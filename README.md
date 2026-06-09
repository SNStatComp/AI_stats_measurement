# AI_stats_measurement
Pilot project for measuring official statistics in AI,

🔗 Live demo: https://ai-stats-measurement.lab.sspcloud.fr/

## Overview
This project explores the reliability of Large Language Models (LLMs) when answering questions based on official statistical data. It compares model responses against trusted sources such as National Statistical Institutes (NSIs).

## Goal
The goal of this project is to assess whether publicly available data from NSIs is machine-readable and findable. The aim is to identify whether NSIs need to take action to improve the machine-readability and discoverability of their data to better support AI systems.

## Features
- Analytics dashboard
Evaluate model performance using metrics such as ARR (Accuracy Rate Ratio) per NSI and per model.
- Response inspection
Load and review all responses for a given prompt.

## Access & Limitations
Submitting new prompts is currently limited to CBS researchers only.
Public users can explore results and existing evaluations.

## Quality badges

[![Quality gate](https://sonarcloud.io/api/project_badges/quality_gate?project=SNStatComp_AI_stats_measurement)](https://sonarcloud.io/summary/new_code?id=SNStatComp_AI_stats_measurement)

# UML diagrams

## component diagram
<img width="1437" height="1087" alt="Scrum Workplace (13)" src="https://github.com/user-attachments/assets/f05213d0-fe5c-4551-b762-c9895ce796b8" />

## service diagram
<img width="1966" height="794" alt="Scrum Workplace (12)" src="https://github.com/user-attachments/assets/344028dd-b351-4dc4-aba4-04ce7f34b016" />

## ERD diagram 

<img width="1693" height="922" alt="Scrum Workplace (14)" src="https://github.com/user-attachments/assets/5e9fed4a-36f5-4c8d-8fa5-9df562993f07" />

# Installation on SSP Cloud

This guide explains how to deploy the **AI Stats Measurement** application on SSP Cloud using Kubernetes and Helm.

## Step 1: Create an SSP Cloud account

Go to:

https://datalab.sspcloud.fr/

Create an account with your NSI account.

## Step 2: Create secrets

Go to your SSP Cloud secrets page

<img width="1179" height="479" alt="image" src="https://github.com/user-attachments/assets/7846a24e-bb6d-42e8-a2ef-cf4b8012654b" />

Name the secret : ai-stats-measurement-secrets

Add the required secrets for the application.

| Variable name                | Description                             |
| ---------------------------- | --------------------------------------- |
| `CONNECTIONSTRINGS__DEFAULT` | PostgreSQL connection string            |
| `IDENTITY__ADMINEMAIL`       | Admin email address for the application |
| `IDENTITY__ADMINPASSWORD`    | Admin password for the application      |
| `JWT__AUDIENCE`              | JWT audience value                      |
| `JWT__ISSUER`                | JWT issuer value                        |
| `JWT__KEY`                   | Secret key used to sign JWT tokens      |
| `LLMKEYS__GEMINI`            | Google Gemini API key                   |
| `LLMKEYS__GROK`              | xAI Grok API key                        |
| `LLMKEYS__OPENAI`            | OpenAI API key                          |
| `POSTGRES_PASSWORD`          | Password for the PostgreSQL database    |

Example values:

```
CONNECTIONSTRINGS__DEFAULT=Host=ai-stats-postgres;Port=5432;Database=AIStatsMeasurementDB;Username=appuser;Password=<your-postgres-password>
IDENTITY__ADMINEMAIL=your-admin-email@example.com
IDENTITY__ADMINPASSWORD=<your-admin-password>
JWT__AUDIENCE=AIStatsUsers
JWT__ISSUER=AIStatsMeasurement
JWT__KEY=<your-jwt-key>
LLMKEYS__GEMINI=<your-gemini-api-key>
LLMKEYS__GROK=<your-grok-api-key>
LLMKEYS__OPENAI=<your-openai-api-key>
POSTGRES_PASSWORD=<your-postgres-password>
```

LLM API keys can be generated here:

- OpenAI: https://platform.openai.com/api-keys
- Google Gemini: https://aistudio.google.com/api-keys
- xAI: https://console.x.ai/team/d3e919c1-6450-4258-a690-15f7085a1b35/api-keys

Generate the JWT key in the terminal:

```bash
openssl rand -base64 32
```

Add this generated value as your JWT secret.

its should look like this 

<img width="1795" height="767" alt="image" src="https://github.com/user-attachments/assets/b628685e-1424-4930-9f1d-c40a350a947b" />

## Step 3: Create a VS Code Python service

Create a new service in SSP Cloud.

Choose the service: vscode-python

<img width="284" height="118" alt="image" src="https://github.com/user-attachments/assets/5ef058e7-ac62-4951-95e5-d1560b8f308c" />

This service has a terminal with the required permissions to deploy the application.

Choose the role: admin

<img width="677" height="145" alt="image" src="https://github.com/user-attachments/assets/0f043eec-905f-46df-b2d9-9739abcc46d8" />

This gives the service the correct rights in your Kubernetes namespace.

use name of the secret you created earlier: ai-stats-measurement-secrets

<img width="1160" height="367" alt="image" src="https://github.com/user-attachments/assets/4a102a88-a45b-449d-a84e-bcb0afbc77f8" />

## Step 4: Open the terminal

Open the terminal inside the VS Code client.

<img width="788" height="700" alt="image" src="https://github.com/user-attachments/assets/36a67ee8-8692-43fe-90c7-aaeae507c938" />

## Step 5: Clone the repository

Download the repository with this command:

```bash
git clone https://github.com/SNStatComp/AI_stats_measurement.git
```

Navigate to the root of the project:
```bash
cd AI_stats_measurement
```

## Step 6: Deploy the application with Helm

Deploy the Helm chart with this command:

```bash
helm upgrade --install ai-stats ./ai-stats-measurement-chart -n user-yourusername
```

## Step 7: Check the pod status

Check if the pods are created:

```bash
kubectl get pods -n user-yourusername
```
If all pods are running, the application has been deployed successfully.

<img width="884" height="174" alt="image" src="https://github.com/user-attachments/assets/60bc1ddc-9a39-4815-849c-e2eb3dce9d35" />

## Optional: Configure CI/CD with GitHub Actions

This step is optional. Use this if you want GitHub Actions to deploy the application automatically.

## Step 1: Create GitHub secrets

In your GitHub repository, go to: Settings > Secrets and variables > Actions

Create the required secrets for the CI/CD pipeline.

<img width="575" height="330" alt="image" src="https://github.com/user-attachments/assets/bc1bbe57-4d02-4650-aa59-94db9c88545b" /> 

Add the following secrets:

| Secret name          | Description                                             | Example value             |
| -------------------- | ------------------------------------------------------- | ------------------------- |
| `DOCKERHUB_USERNAME` | Your Docker Hub username                                | `your-dockerhub-username` |
| `DOCKERHUB_TOKEN`    | Docker Hub access token used to push images             | `<your-dockerhub-token>`  |
| `KUBE_NAMESPACE`     | Your SSP Cloud Kubernetes namespace                     | `user-yourusername`       |
| `KUBE_SERVER`        | Kubernetes API server URL from SSP Cloud                | `https://apiserver.kub.sspcloud.fr` |
| `KUBE_TOKEN`         | Kubernetes service account token used by GitHub Actions | `<your-kubernetes-token>` |
| `SONAR_TOKEN`        | SonarCloud token used for code analysis                 | `<your-sonar-token>`      |


## Step 2: Create a Kubernetes service account

To allow GitHub Actions to deploy the application, create a service account in Kubernetes.

```bash
kubectl apply -f sa-token.yaml
```

## Step 3: Retrieve the service account token

After applying **sa-token.yaml**, retrieve the token and add it to GitHub Secrets.

Check the created secret:

```bash
kubectl describe secret github-actions-token -n user-yourusername
```

Copy the token and add it as a GitHub secret.

# Usefull Kubernetes / SSP Cloud commands

## Check pods

```bash
kubectl get pods -n user-yourusername
kubectl get pods -n user-yourusername -w
```

## Check services

```bash
kubectl get svc -n user-yourusername
```

## Check ingress

```bash
kubectl get ingress -n user-yourusername
```

## Check logs

```bash
kubectl logs -f deployment/ai-stats-backend -n user-yourusername
kubectl logs --tail=200 deployment/ai-stats-backend -n user-yourusername
```

## Restart deployment

```bash
kubectl rollout restart deployment ai-stats-backend -n user-yourusername
```

## Deploy or update with Helm

```bash
helm upgrade --install ai-stats ./ai-stats-measurement-chart -n user-yourusername
```

## Uninstall Helm release

```bash
helm uninstall ai-stats -n user-yourusername
```

## Delete pod

```bash
kubectl delete pod ai-stats-postgres-0 -n user-yourusername
```

## Delete database PVC 

```bash
kubectl delete pvc postgres-data-ai-stats-postgres-0 -n user-yourusername
```

## Fully reset app and database

```bash
helm uninstall ai-stats -n user-diegoespinosa
kubectl delete pvc postgres-data-ai-stats-postgres-0 -n user-yourusername
helm upgrade --install ai-stats ./ai-stats-measurement-chart -n user-yourusername
```

## Check secrets

```bash
kubectl get secrets -n user-yourusername
kubectl describe secret ai-stats-measurement-secrets -n user-yourusername
```

## After pushing a new latest image

```bash
kubectl rollout restart deployment ai-stats-backend -n user-yourusername
kubectl rollout restart deployment ai-stats-frontend -n user-yourusername
```

## Pause
```bash
kubectl scale deployment ai-stats-backend --replicas=0 -n user-yourusername
kubectl scale deployment ai-stats-frontend --replicas=0 -n user-yourusername
kubectl scale statefulset ai-stats-postgres --replicas=0 -n user-yourusername
```

## Resume
```bash
kubectl scale statefulset ai-stats-postgres --replicas=1 -n user-yourusername
kubectl scale deployment ai-stats-backend --replicas=1 -n user-yourusername
kubectl scale deployment ai-stats-frontend --replicas=1 -n user-yourusername
```
