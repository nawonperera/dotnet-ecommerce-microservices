# Kubernetes Service Guide: MongoDB

Welcome back! Just like the Deployment file, we have stripped the comments out of the `mongodb.service.yaml` file to make it clean and readable.

> **Note on Kubernetes Concepts:**
> A **Service** provides a stable network endpoint (like a persistent name and IP address) for accessing your Pods. Even if a Pod crashes and Kubernetes creates a new one with a new IP address, the Service keeps the same name. This allows other applications to reliably connect to it.

---

## Full Clean Code

```yaml
apiVersion: v1
kind: Service
metadata:
  name: mongodb
  namespace: ecommerce-namespace
spec:
  selector:
    app: mongodb
  ports:
    - protocol: TCP
      port: 27017
      targetPort: 27017
  type: ClusterIP
```

---

## Step-by-Step Breakdown

### 1. API Version & Kind

```yaml
apiVersion: v1
kind: Service
```

- **`apiVersion`**: Specifies the Kubernetes API version used to create this resource. `v1` is the core version used for standard resources like Services, Pods, ConfigMaps, and Secrets.
- **`kind`**: This tells Kubernetes we are creating a **Service**. Services make sure that your application is reachable over the network reliably.

### 2. Metadata

```yaml
metadata:
  name: mongodb
  namespace: ecommerce-namespace
```

- **`metadata`**: Identifying information about this Service.
- **`name`**: The name of the Service. Because we named it `mongodb`, other applications inside the cluster can simply connect to `mongodb:27017` to reach the database, without ever needing to know its IP address.
- **`namespace`**: Just like with the Deployment, this ensures the Service is created in the `ecommerce-namespace` virtual cluster, allowing it to communicate with the Pods in that same namespace.

### 3. Selector (How it finds Pods)

```yaml
spec:
  selector:
    app: mongodb
```

- **`spec`**: Defines how the Service behaves.
- **`selector`**: This is how the Service knows which Pods it should route traffic to. It acts like a search filter. It tells Kubernetes: "Find any Pods with the label `app: mongodb` and send traffic to them." This matches the labels we set in our Deployment file!

### 4. Ports Mapping

```yaml
  ports:
    - protocol: TCP
      port: 27017
      targetPort: 27017
```

- **`ports`**: Defines the network ports for the Service.
- **`protocol`**: We are using TCP for MongoDB communication.
- **`port`**: This is the port the *Service* listens on. Other apps in the cluster connect to this port.
- **`targetPort`**: This is the port on the *container* inside the Pod where MongoDB is actually listening. Traffic arriving at the Service on `port: 27017` is forwarded directly to `targetPort: 27017` inside the Pod.

### 5. Service Type

```yaml
  type: ClusterIP
```

- **`type`**: Determines how the Service is exposed. `ClusterIP` is the default type.
- **`ClusterIP`**: This creates an internal IP address that is *only* reachable from within the Kubernetes cluster. This means other microservices can connect to the database, but no one on the public internet can access it. This is exactly what you want for a secure database!

---

## How to use this file
To apply this file and create the Service in your Kubernetes cluster, run the following command in your terminal:
```bash
kubectl apply -f mongodb.service.yaml
```
