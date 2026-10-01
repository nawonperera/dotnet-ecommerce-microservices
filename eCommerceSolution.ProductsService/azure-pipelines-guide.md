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
          latest
        buildContext: $(Build.SourcesDirectory)
```
- `steps`: The linear sequence of operations that make up a job.
- `task: Docker@2`: Uses version 2 of the official Azure DevOps Docker task.
- `command: buildAndPush`: Combines both the `docker build` and `docker push` commands into a single action.
- `repository`: Sets the name of the container repository using the `$(imageRepository)` variable.
- `dockerfile`: The exact file path to the `Dockerfile`.
- `containerRegistry`: Passes the service connection ID to authenticate with the container registry. 
- `tags`: Sets the tag on the built image using the `$(tag)` variable, which resolves to the build ID. Because of the `|`, Azure DevOps reads the tags input as a list with two separate lines. When the pipeline runs, it will build your image and push it to Azure Container Registry with all two of those tags attached to it.(nawonecommerceregistry.azurecr.io/products-microservice:12345, nawonecommerceregistry.azurecr.io/products-microservice:latest)
This means a system could pull products-microservice:latest to always get the newest code, or pull products-microservice:12345 if it needs to roll back to that exact specific build.
- `buildContext`: Tells the Docker build process to use the root of the source directory as its context, allowing it to reference files correctly during the build.(This is the solution's folder.In this case it's `eCommerceSolution.ProductsService`)

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
- `checkout: self`: Explicitly tells the agent to download the repository source code so it is available for testing. This tells the pipeline that its primary repository is the exact same one hosting this YAML file.

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
- `arguments: '--collect:"Code Coverage"'`: Instructs the test runner to generate code coverage metrics during execution.(Check how much of my code is actually being tested.)
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
- `condition: and(...)`: This acts as a strict security guard before deployment, ensuring two rules are met. Let's break down each piece:
  - `and(...)`: Means *both* of the rules inside the parentheses must be true.
  - `succeeded('Build')`: Rule #1. It checks the history to make sure the previous `Build` stage was 100% successful.
  - `eq(...)`: Stands for "equals". It compares two things to see if they are an exact match.
  - `variables['Build.SourceBranch']`: This is the first thing being compared. It is a system variable that asks "Which branch triggered this pipeline?"
  - `'refs/heads/dev'`: This is the second thing being compared. It is the full system name for your `dev` branch.
  - **Summary**: "Only run this deployment if the Build succeeded AND the code came exactly from the dev branch."

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

            - script: |
                find $(Build.SourcesDirectory)/k8s/dev -type f \( -name "*.yaml" -o -name "*.yml" \) -exec sed -i 's/__TAG__/$(tag)/g' {} +
              displayName: 'Replace image tag in all deployment files'

            - task: Kubernetes@1
              displayName: Deploy to dev namespace in kubernetes
              inputs:
                kubernetesServiceEndpoint: $(devAksServiceConnectionName)
                kubernetesCluster: $(aksClusterName)
                namespace: $(devKubernetesNamespace)
                command: apply
                arguments: '-f $(Build.SourcesDirectory)/k8s/dev/.'
```
- `deploy`: A lifecycle hook within the deployment strategy that holds the steps to execute.
- `script`: Executes a raw Bash script on the agent.
- `ls -l $(Build.SourcesDirectory)/k8s/`: A diagnostic command that prints the contents of the `k8s` folder to the pipeline logs to verify the deployment manifests are present on the agent.
- `script (Replace image tag)`: Executes a `find` command combined with `sed` to locate every `.yaml` or `.yml` file in the `k8s/dev` directory and dynamically replace the `__TAG__` placeholder with the actual build ID (`$(tag)`). This ensures Kubernetes deploys the exact container image we just built.
- `task: Kubernetes@1`: Uses the official Azure DevOps Kubernetes task to interact with the AKS cluster.
- `kubernetesServiceEndpoint`: Authenticates to the cluster using the service connection stored in the variable.
- `kubernetesCluster`: Specifies the target AKS cluster (`$(aksClusterName)`).
- `namespace`: Specifies the target namespace inside the cluster where the resources will be deployed.
- `command: apply`: Maps to the `kubectl apply` command, instructing Kubernetes to create or update resources based on the provided manifest files.
- `arguments: '-f $(Build.SourcesDirectory)/k8s/dev/.'`: Passes the folder containing the dynamically updated Kubernetes YAML files to the `apply` command, telling the cluster to execute all of them at once.
