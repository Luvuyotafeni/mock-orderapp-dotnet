 Firstly Build Images with nerdctl

nerdctl --namespace k8s.io build -t service-1:1.0 ./service-1 (directory of the dockerfile)
nerdctl --namespace k8s.io build -t service-2:1.0 ./service-2 (directory of the dockerfile)
nerdctl --namespace k8s.io build -t service-3:1.0 ./service-3 (directory of the dockerfile)
nerdctl --namespace k8s.io build -t service-4:1.0 ./service-4 (directory of the dockerfile)


Then now create the yaml file 
kubectl apply projec.yaml