# Azure Pipelines Guide: Products Service

This guide provides a detailed, line-by-line technical breakdown of the `azure-pipelines.yml` file. It explains exactly what each parameter does in the context of Azure DevOps.

---

### 1. Pipeline Triggers

```yaml
trigger:
- dev
```
- `trigger`: Defines which branches will automatically start a pipeline run when code is pushed to them.
- `- dev`: Specifies that the pipeline will trigger automatically on pushes to the `dev` branch.

### 2. Repository Resources

```yaml
resources:
- repo: self
```
- `resources`: Defines external resources the pipeline will consume.
- `- repo: self`: Instructs the pipeline to use the source code from the repository where this YAML file is located.

### 3. Pipeline Variables

```yaml
variables:
  - name: dockerRegistryServiceConnection
    value: 'fb67587d-6211-425e-ba68-7261394b0a1c'
  - name: imageRepository
    value: 'products-microservice'
  - name: containerRegistry
    value: 'nawonecommerceregistry.azurecr.io'
  - name: dockerfilePath
    value: '$(Build.SourcesDirectory)/ProductsMicroServiceAPI/Dockerfile'
  - name: tag
    value: '$(Build.BuildId)'
  - name: vmImageName
    value: 'ubuntu-latest'
  - name: windowImageName
    value: 'windows-latest'
  - name: aksClusterName
    value: 'ecommerce-aks-cluster'
  - name: aksServiceConnectionName
    value: 'dev-ecommerce-aks-cluster-dev-1728475525366'
  - name: deploymentFile
    value: '$(Build.SourcesDirectory)/k8s/products-microservice-dev-deployment.yaml'
  - name: kubernetesNamespace
    value: 'dev'
```
- `variables`: A block used to define reusable key-value pairs to avoid hardcoding values throughout the pipeline.
- `dockerRegistryServiceConnection`: The GUID for the service connection that securely authenticates Azure DevOps with the Azure Container Registry.
- `imageRepository`: The name that will be given to the Docker image (`products-microservice`).
- `containerRegistry`: The URL of the Azure Container Registry where the image will be stored.
- `dockerfilePath`: The absolute path to the Dockerfile, dynamically built using the system variable `$(Build.SourcesDirectory)`.
- `tag`: The version tag for the Docker image. `$(Build.BuildId)` automatically uses the unique ID of the current pipeline run.
- `vmImageName` / `windowImageName`: The VM images used to run the pipeline agents. We define both an Ubuntu agent and a Windows agent for different stages.
- `aksClusterName` / `aksServiceConnectionName`: The target Azure Kubernetes Service cluster and its authentication connection.
- `deploymentFile`: The location of the Kubernetes manifest file to deploy.
- `kubernetesNamespace`: The target namespace (`dev`) in the Kubernetes cluster.

---

### 4. Stage 1: Build

```yaml
stages:
- stage: Build
  displayName: Build and push stage
  jobs:
  - job: Build
    displayName: Docker Build
    pool:
      vmImage: $(vmImageName)
```
- `stages`: The highest level of organization in a pipeline, representing major phases of the CI/CD process.
- `stage: Build`: Defines a stage explicitly named "Build".
- `displayName`: The human-readable label displayed in the Azure DevOps UI.
- `jobs`: A collection of tasks that run sequentially on the same agent.
- `pool`: Specifies the agent pool to use.
- `vmImage: $(vmImageName)`: Resolves to `ubuntu-latest`, meaning this job runs on a Microsoft-hosted Linux agent.

```yaml
    steps:
    - task: Docker@2
      displayName: Build and push an image to container registry
      inputs:
        command: buildAndPush
        repository: $(imageRepository)
        dockerfile: $(dockerfilePath)
        containerRegistry: $(dockerRegistryServiceConnection)
        tags: |
          $(tag)
        buildContext: $(Build.SourcesDirectory)
```
- `steps`: The linear sequence of operations that make up a job.
- `task: Docker@2`: Uses version 2 of the official Azure DevOps Docker task.
- `command: buildAndPush`: Combines both the `docker build` and `docker push` commands into a single action.
- `repository`: Sets the name of the container repository using the `$(imageRepository)` variable.
- `dockerfile`: The exact file path to the `Dockerfile`.
- `containerRegistry`: Passes the service connection ID to authenticate with the container registry. 
- `tags`: Sets the tag on the built image using the `$(tag)` variable, which resolves to the build ID.
- `buildContext`: Tells the Docker build process to use the root of the source directory as its context, allowing it to reference files correctly during the build.

---

### 5. Stage 2: Test

```yaml
- stage: Test
  displayName: Test and Publish results stage
  jobs:
    - job: RunTest
      displayName: Run Unit Tests
      pool:
        vmImage: $(windowImageName)
```
- `pool: vmImage: $(windowImageName)`: This stage switches to a Windows agent (`windows-latest`) instead of Linux to run the .NET tests.

```yaml
      steps:
        - checkout: self
```
- `checkout: self`: Explicitly tells the agent to download the repository source code so it is available for testing.

```yaml
        - task: NuGetToolInstaller@1
          displayName: Install NuGetTool
```
- `task: NuGetToolInstaller@1`: Downloads and installs the NuGet tool on the agent, ensuring the correct version is available to restore packages.

```yaml
        - task: DotNetCoreCLI@2
          displayName: Restore NuGet Packages
          inputs:
            command: 'restore'
            projects: '**/eCommerceSolution.ProductsService.slnx'
```
- `task: DotNetCoreCLI@2`: Uses the official .NET Core CLI task.
- `command: 'restore'`: Executes `dotnet restore` to download all project dependencies.
- `projects`: Uses a wildcard pattern to find the `.slnx` solution file regardless of its exact folder depth.

```yaml
        - task: DotNetCoreCLI@2
          displayName: Build Solution
          inputs:
            command: 'build'
            projects: '**/eCommerceSolution.ProductsService.slnx'
            arguments: '--configuration Debug'
            configuration: 'debug'
```
- `command: 'build'`: Executes `dotnet build` to compile the solution.
- `arguments: '--configuration Debug'`: Passes the Debug flag to the compiler.

```yaml
        - task: DotNetCoreCLI@2
          displayName: Run Unit Tests
          inputs:
            command: 'test'
            projects: '**/eCommerceSolution.ProductsService.slnx'
            arguments: '--configuration Release --collect:"Code Coverage"'
            publishTestResults: true
```
- `command: 'test'`: Executes `dotnet test` to run all unit tests in the solution.
- `arguments: '--collect:"Code Coverage"'`: Instructs the test runner to generate code coverage metrics during execution.
- `publishTestResults: true`: Automatically parses the test results and uploads them to the Azure DevOps test reporting dashboard.

---

### 6. Stage 3: Deploy to Dev

```yaml
- stage: DeployToDev
  displayName: Deploy to Dev
  dependsOn: Test
  condition: and(succeeded('Build'), eq(variables['Build.SourceBranch'], 'refs/heads/dev'))
```
- `dependsOn: Test`: Forces this stage to wait until the `Test` stage is completely finished. By default, stages run in parallel unless a dependency is specified.
- `condition: ...`: Evaluates a custom expression to decide if the stage should run.
- `succeeded('Build')`: Ensures the stage only runs if the `Build` stage passed.
- `eq(variables['Build.SourceBranch'], 'refs/heads/dev')`: Ensures the deployment only happens if the pipeline was triggered by the `dev` branch.

```yaml
  jobs:
  - deployment: DeploymentToDev
    displayName: Deployment to Dev Environment
    environment: dev
    strategy:
      runOnce:
```
- `deployment`: A special type of job specifically designed for deployment operations. It enables deployment history and environment tracking in Azure DevOps.
- `environment: dev`: Targets the logical "dev" environment in Azure DevOps, allowing you to set up approvals or track deployment history.
- `strategy: runOnce`: Specifies the deployment strategy. `runOnce` means it will execute all lifecycle hooks just one time.

```yaml
        deploy:
            steps:
            - checkout: self
            - script: |
                echo "Listing contents of k8s"
                ls -l $(Build.SourcesDirectory)/k8s/
              displayName:  'List Files in k8s Directory'
```
- `deploy`: A lifecycle hook within the deployment strategy that holds the steps to execute.
- `script`: Executes a raw Bash script on the agent.
- `ls -l $(Build.SourcesDirectory)/k8s/`: A diagnostic command that prints the contents of the `k8s` folder to the pipeline logs to verify the deployment manifests are present on the agent.
