# Azure Pipelines Guide: Products Service

This guide provides a detailed, line-by-line technical breakdown of the `azure-pipelines.yml` file. It explains exactly what each parameter does in the context of Azure DevOps.

---

### 1. Pipeline Triggers

```yaml
trigger:
  branches:
    include:
      - dev
      - qa
      - uat
      - staging
      - production
```
- `trigger`: Defines which branches will automatically start a pipeline run when code is pushed to them.
- `branches.include`: Explicitly lists the branches that are allowed to trigger the pipeline. Pushes to `dev`, `qa`, `uat`, `staging`, or `production` will now automatically kick off the build and deployment process.

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
  # ... (Docker variables)
  - name: linuxImageName
    value: 'ubuntu-latest'
  - name: windowsImageName
    value: 'windows-latest'
  - name: aksClusterName
    value: 'ecommerce-aks-cluster'
  # ... (Environment specific Service Connections and Namespaces)
```
- `variables`: A block used to define reusable key-value pairs to avoid hardcoding values throughout the pipeline.
- `linuxImageName` / `windowsImageName`: The VM images used to run the pipeline agents. (Notice these were renamed to differentiate the OS types).
- **Environment Connections & Namespaces**: The pipeline now defines a separate Azure Kubernetes Service (AKS) connection ID and a target Kubernetes namespace (`dev`, `qa`, `uat`, `staging`, `prod`) for each stage of the lifecycle. This isolates the deployments from each other securely.

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
      vmImage: $(linuxImageName)
```
- `stages`: The highest level of organization in a pipeline, representing major phases of the CI/CD process.
- `stage: Build`: Defines a stage explicitly named "Build".
- `pool`: Specifies the agent pool to use.
- `vmImage: $(linuxImageName)`: Resolves to `ubuntu-latest`, meaning this job runs on a Microsoft-hosted Linux agent.

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
- `task: Docker@2`: Uses version 2 of the official Azure DevOps Docker task.
- `command: buildAndPush`: Combines both the `docker build` and `docker push` commands into a single action.
- `tags`: Sets the tag on the built image using the `$(tag)` variable (the build ID) as well as the `latest` tag. The `|` allows passing a multi-line list.

---

### 5. Stage 2: Test

```yaml
- stage: Test
  displayName: Test and Publish results stage
  jobs:
    - job: RunTest
      displayName: Run Unit Tests
      pool:
        vmImage: $(windowsImageName)
```
- `pool: vmImage: $(windowsImageName)`: This stage switches to a Windows agent (`windows-latest`) instead of Linux to run the .NET tests.

```yaml
      steps:
        - checkout: self
        - task: NuGetToolInstaller@1
        - task: DotNetCoreCLI@2
          inputs:
            command: 'restore'
            projects: '**/eCommerceSolution.ProductsService.slnx'
        - task: DotNetCoreCLI@2
          inputs:
            command: 'build'
            projects: '**/eCommerceSolution.ProductsService.slnx'
            arguments: '--configuration Debug'
        - task: DotNetCoreCLI@2
          inputs:
            command: 'test'
            projects: '**/eCommerceSolution.ProductsService.slnx'
            arguments: '--configuration Release --collect:"Code Coverage"'
            publishTestResults: true
```
- `checkout: self`: Explicitly downloads the repository source code.
- `NuGetToolInstaller@1` / `DotNetCoreCLI@2`: Restores dependencies, builds the solution in Debug mode, and finally runs unit tests in Release mode.
- `publishTestResults: true`: Automatically parses the test results and uploads them to the Azure DevOps test reporting dashboard.

---

### 6. Stages 3-7: Multi-Environment Deployments

The pipeline now contains multiple deployment stages (`DeployToDev`, `DeployToQA`, `DeployToUAT`, `DeployToStaging`, `DeployToProduction`). Each stage functions identically but targets a different environment. Here is a breakdown of the standard deployment pattern used for all of them:

```yaml
- stage: DeployToDev
  displayName: Deploy to Dev
  dependsOn: Test
  condition: and(succeeded('Build'), eq(variables['Build.SourceBranch'], 'refs/heads/dev'))
```
- `dependsOn: Test`: Forces this stage to wait until the `Test` stage is completely finished.
- `condition: and(...)`: This acts as a strict security guard before deployment, ensuring two rules are met. Let's break down each piece:
  - `and(...)`: Means *both* of the rules inside the parentheses must be true.
  - `succeeded('Build')`: Rule #1. It checks the history to make sure the previous `Build` stage was 100% successful.
  - `eq(...)`: Stands for "equals". It compares two things to see if they are an exact match.
  - `variables['Build.SourceBranch']`: This is the first thing being compared. It is a system variable that asks "Which branch triggered this pipeline?"
  - `'refs/heads/dev'`: This is the second thing being compared. *(Note: For QA, this is 'refs/heads/qa', for UAT 'refs/heads/uat', etc.)*
  - **Summary**: "Only run this deployment if the Build succeeded AND the code came exactly from the dev branch."

```yaml
  jobs:
  - deployment: DeploymentToDev
    environment: dev
    strategy:
      runOnce:
```
- `deployment`: A special type of job specifically designed for deployment operations. It enables deployment history and environment tracking in Azure DevOps.
- `environment: dev`: Targets the logical "dev" environment in Azure DevOps, allowing you to set up approvals or track deployment history. *(Note: This changes to qa, uat, staging, or prod depending on the stage).*
- `strategy: runOnce`: Specifies the deployment strategy. `runOnce` means it will execute all lifecycle hooks just one time.

```yaml
        deploy:
            steps:
            - checkout: self
            - script: |
                echo "Listing contents of k8s"
                ls -l $(Build.SourcesDirectory)/k8s/dev
              displayName:  'List Files in k8s/dev Directory'

            - script: |
                find $(Build.SourcesDirectory)/k8s/dev -type f \( -name "*.yaml" -o -name "*.yml" \) -exec sed -i 's/__TAG__/$(tag)/g' {} +
              displayName: 'Replace image tag in all deployment files'
```
- `deploy`: A lifecycle hook within the deployment strategy that holds the steps to execute.
- `script (List Files)`: Executes a raw Bash script on the agent to run `ls -l` as a diagnostic command. It prints the contents of the environment's `k8s` folder.
- `script (Replace image tag)`: Executes a bash script to dynamically replace the `__TAG__` placeholder in the Kubernetes deployment files with the actual build ID (`$(tag)`). Let's break down this specific bash command:
  - `find $(Build.SourcesDirectory)/k8s/dev`: Instructs the `find` utility to start searching inside the directory where the pipeline downloaded the Kubernetes manifests.
  - `-type f`: Restricts the search results to only return files (ignoring directories).
  - `\( -name "*.yaml" -o -name "*.yml" \)`: Tells `find` to only look for files ending with the `.yaml` OR (`-o`) `.yml` extensions.
  - `-exec ... {} +`: Takes all the files located by the `find` command and passes them as arguments (`{}`) to the command immediately following `-exec`.
  - `sed -i`: Invokes the stream editor (`sed`) and uses the `-i` flag to edit the files "in-place", meaning it directly modifies and overwrites the original files rather than just printing the output to the console.
  - `'s/__TAG__/$(tag)/g'`: This is the exact `sed` substitution command being executed on the files. It instructs `sed` to **s**ubstitute the literal text string `__TAG__` with the value of the Azure DevOps variable `$(tag)`, and to do it **g**lobally (every time it appears on a line).

```yaml
            - task: Kubernetes@1
              displayName: Deploy to dev namespace in kubernetes
              inputs:
                kubernetesServiceEndpoint: $(devAksServiceConnectionName)
                kubernetesCluster: $(aksClusterName)
                namespace: $(devKubernetesNamespace)
                command: apply
                arguments: '-f $(Build.SourcesDirectory)/k8s/dev/.'
```
- `task: Kubernetes@1`: Uses the official Azure DevOps Kubernetes task to interact with the AKS cluster.
- `kubernetesServiceEndpoint`: Authenticates to the cluster using the service connection stored in the variable (`devAksServiceConnectionName`, `qaAksServiceConnectionName`, etc.).
- `namespace`: Specifies the target namespace inside the cluster where the resources will be deployed (`dev`, `qa`, `uat`, etc.).
- `command: apply`: Maps to the `kubectl apply` command, instructing Kubernetes to create or update resources based on the provided manifest files.
- `arguments`: Passes the environment-specific folder containing the dynamically updated Kubernetes YAML files to the `apply` command.
