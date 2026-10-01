# Kubernetes Deployment Guide: Products Microservice (Dev)

This guide provides a detailed, line-by-line technical breakdown of the `products-microservice-dev-deployment.yaml` file. It explains exactly what each parameter does in the context of Kubernetes deployments.

---

### 1. API Version and Resource Type

```yaml
apiVersion: apps/v1
kind: Deployment
```
- `apiVersion: apps/v1`: Specifies the Kubernetes API version used to create this resource. The `apps/v1` API group is the stable version specifically used for workload resources like Deployments, StatefulSets, and DaemonSets.
- `kind: Deployment`: Declares the type of Kubernetes resource being created. A Deployment manages a set of identical Pods, ensuring they are running, replacing them if they fail, and managing updates or rollbacks.

### 2. Metadata

```yaml
metadata:
  name: products-microservice-deployment
  labels:
    app: products-microservice
```
- `metadata`: Contains identifying information that helps organize and locate the deployment within the cluster.
- `name: products-microservice-deployment`: The unique name of this Deployment resource.
- `labels`: Key-value pairs used to organize resources.
- `app: products-microservice`: A tag applied to the Deployment itself, typically used for querying or filtering resources using `kubectl`.

### 3. Deployment Specification (Desired State)

```yaml
spec:
  replicas: 1
  selector:
    matchLabels:
      app: products-microservice
```
- `spec`: Defines the desired state for the Deployment. Kubernetes will continuously monitor the cluster to ensure reality matches this specification.
- `replicas: 1`: Instructs the Deployment controller to ensure exactly one instance (Pod) of this application is running at all times.
- `selector`: Dictates how the Deployment identifies which Pods it is responsible for managing.
- `matchLabels`: Specifies that the Deployment will manage any Pods that have the exact labels listed under it.
- `app: products-microservice`: The specific label the Deployment looks for. If a Pod has this label, the Deployment assumes ownership of it.

### 4. Pod Template Metadata

```yaml
  template:
    metadata:
      labels:
        app: products-microservice
```
- `template`: The blueprint that the Deployment uses to stamp out new Pods.
- `metadata.labels`: The labels that will be attached to every new Pod created from this template.
- `app: products-microservice`: This label is crucial because it exactly matches the `selector` defined in the previous block. This ensures the Deployment correctly recognizes the Pods it just created.

### 5. Pod Specification (Containers)

```yaml
    spec:
      containers:
      - name: products-microservice
        image: nawonecommerceregistry.azurecr.io/products-microservice:__TAG__
```
- `spec`: Describes the contents and configuration of the Pod itself.
- `containers`: A list defining the containers that will run inside this Pod.
- `name: products-microservice`: The human-readable name of the container running inside the Pod.
- `image`: The exact Docker image to pull and run. Notice the `:__TAG__` at the end; this is a placeholder that is typically dynamically replaced by a CI/CD pipeline (like Azure Pipelines) with the actual build version before applying the file to the cluster.

### 6. Container Ports

```yaml
        ports:
        - containerPort: 8080
```
- `ports`: A list of network ports exposed by the container.
- `containerPort: 8080`: Documents that the application inside the container is actively listening on TCP port 8080. (This matches the `targetPort` we saw in the Service file).

### 7. Environment Variables

```yaml
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: Development
        - name: MYSQL_HOST
          value: mysql
        # ... (other MySQL variables)
        - name: RabbitMQ_HostName
          value: rabbitmq
        # ... (other RabbitMQ variables)
```
- `env`: An array of environment variables that are injected directly into the running container. This allows the application to be configured dynamically without changing the code.
- `ASPNETCORE_ENVIRONMENT`: Tells the .NET runtime to run in "Development" mode, which usually enables detailed error pages and debug logging.
- `MYSQL_*`: Variables that configure the database connection. `MYSQL_HOST: mysql` indicates that the app will connect to another Kubernetes Service named `mysql` to reach the database.
- `RabbitMQ_*`: Variables configuring the connection to the RabbitMQ message broker. `RabbitMQ_HostName: rabbitmq` means the app will route messages to another Kubernetes Service internally named `rabbitmq`.
