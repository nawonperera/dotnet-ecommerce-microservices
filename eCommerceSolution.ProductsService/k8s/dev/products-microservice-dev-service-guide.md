# Kubernetes Service Guide: Products Microservice (Dev)

This guide provides a detailed, line-by-line technical breakdown of the `products-microservice-dev-service.yaml` file. It explains exactly what each parameter does in the context of Kubernetes.

---

### 1. API Version and Resource Type

```yaml
apiVersion: v1
kind: Service
```
- `apiVersion: v1`: Specifies the Kubernetes API version used to create this resource. `v1` is the core API group used for standard, fundamental resources like Services, Pods, and ConfigMaps.
- `kind: Service`: Declares the type of Kubernetes resource being created. A Service provides a stable, persistent network endpoint (IP address and DNS name) to access a dynamic set of Pods.

### 2. Metadata

```yaml
metadata:
  name: products-microservice
```
- `metadata`: Contains identifying information that helps organize and locate the resource within the cluster.
- `name: products-microservice`: The unique name of the Service. Because it is named `products-microservice`, other applications running inside the same Kubernetes cluster can communicate with this service by simply using `http://products-microservice:8080` as the URL, relying on Kubernetes' internal DNS.

### 3. Service Specification

```yaml
spec:
  selector:
    app: products-microservice
```
- `spec`: Defines the desired state and behavior of the Service.
- `selector`: The filtering mechanism the Service uses to determine which Pods it should route traffic to.
- `app: products-microservice`: Instructs the Service to look for any running Pods that have the label `app` set to `products-microservice`. When requests hit this Service, it will act as a load balancer, forwarding the traffic to those matching Pods.

### 4. Port Configuration

```yaml
  ports:
    - protocol: TCP
      port: 8080
      targetPort: 8080
```
- `ports`: A list defining the network ports exposed by the Service.
- `protocol: TCP`: Specifies that the Service will handle Transmission Control Protocol traffic (the standard protocol for HTTP/web traffic).
- `port: 8080`: The port that this **Service** listens on. Other microservices within the cluster will send their requests to port `8080` on the Service's IP.
- `targetPort: 8080`: The port on the **Pod's container** where the application is actually listening. The Service receives traffic on `port`, and forwards it to `targetPort` inside the Pod. (In this case, both are `8080`).

### 5. Service Type

```yaml
  type: ClusterIP
```
- `type: ClusterIP`: Defines how the Service is exposed. `ClusterIP` is the default Service type in Kubernetes. It provisions an internal IP address that is **only** accessible from within the Kubernetes cluster. This means the products microservice is securely hidden from the public internet and can only be accessed by other microservices deployed inside the same cluster.
