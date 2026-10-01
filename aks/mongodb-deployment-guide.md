# Kubernetes Deployment Guide: MongoDB

Welcome to Kubernetes! As a beginner, reading YAML files with too many comments can be overwhelming, so we stripped the comments out of the `mongodb-deployment.yaml` file to make it clean and readable. 

To help you understand what every part of the file does without cluttering the code, here is a breakdown of the file.

> **Note on Kubernetes Concepts:**
> A **Deployment** is a resource in Kubernetes that manages a set of identical Pods (which run your containers). It ensures that the correct number of Pods are running at all times.

---

## Full Clean Code

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: mongodb-deployment
  namespace: ecommerce-namespace
  labels:
    app: mongodb
spec:
  replicas: 1
  selector:
    matchLabels:
      app: mongodb
  template:
    metadata:
      labels:
        app: mongodb
    spec:
      containers:
      - name: mongodb
        image: nawonecommerceregistry.azurecr.io/ecommerce-mongodb:latest
        ports:
        - containerPort: 27017
```

---

## Step-by-Step Breakdown

### 1. API Version & Kind

```yaml
apiVersion: apps/v1
kind: Deployment
```

- **`apiVersion`**: Every Kubernetes object needs an API version to specify which version of the Kubernetes API to use. `apps/v1` is the stable version for Deployments.
- **`kind`**: This tells Kubernetes what type of resource you are trying to create. Here, we are creating a **Deployment**. Deployments manage the lifecycle of your application (like starting, restarting, or scaling up).

### 2. Metadata

```yaml
metadata:
  name: mongodb-deployment
  namespace: ecommerce-namespace
  labels:
    app: mongodb
```

- **`metadata`**: Data that helps uniquely identify the object.
- **`name`**: The name of your Deployment. You can use this name to check on it later (e.g., `kubectl get deployment mongodb-deployment`).
- **`namespace`**: Think of this as a virtual cluster or folder. It groups related resources together (in this case, `ecommerce-namespace`).
- **`labels`**: Key-value pairs used to organize and select subsets of objects. This label `app: mongodb` acts as a tag.

### 3. Specification (Desired State)

```yaml
spec:
  replicas: 1
  selector:
    matchLabels:
      app: mongodb
```

- **`spec`**: This section defines the **desired state** for the Deployment.
- **`replicas: 1`**: You are telling Kubernetes you always want exactly 1 Pod (instance) of MongoDB running. If it crashes, Kubernetes will automatically start a new one to maintain this count.
- **`selector`**: This tells the Deployment how to find the Pods it is supposed to manage. It looks for Pods that have the label `app: mongodb`.

### 4. Pod Template (The Blueprint)

```yaml
  template:
    metadata:
      labels:
        app: mongodb
```

- **`template`**: This is the blueprint used to create the Pods. Every time the Deployment needs to create a new Pod, it follows this template.
- **`metadata.labels`**: These are the labels assigned to the Pods created by this template. Notice how it matches the `selector` from the previous section! This is how the Deployment knows these Pods belong to it.

### 5. Container Specification

```yaml
    spec:
      containers:
      - name: mongodb
        image: nawonecommerceregistry.azurecr.io/ecommerce-mongodb:latest
        ports:
        - containerPort: 27017
```

- **`containers`**: A list of containers that will run inside the Pod.
- **`name`**: The friendly name for this specific container.
- **`image`**: The exact Docker image to download and run. In this case, it pulls from your Azure Container Registry (`nawonecommerceregistry.azurecr.io`) using the `latest` tag.
- **`ports`**: Documents which ports the container is listening on. `27017` is the default port for MongoDB. (Note: This doesn't expose it to the internet; it just tells Kubernetes which port the app uses internally).

---

## How to use this file
To apply this file and create the deployment in your Kubernetes cluster, you would run the following command in your terminal:
```bash
kubectl apply -f mongodb-deployment.yaml
```
