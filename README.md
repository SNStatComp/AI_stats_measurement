# AI_stats_measurement
Pilot project for measuring official statistics in AI,

🔗 Live demo: https://ai-stats-measurement.lab.sspcloud.fr/

# Overview
This project explores the reliability of Large Language Models (LLMs) when answering questions based on official statistical data. It compares model responses against trusted sources such as National Statistical Institutes (NSIs).

# Goal
The goal of this project is to assess whether publicly available data from NSIs is machine-readable and findable. The aim is to identify whether NSIs need to take action to improve the machine-readability and discoverability of their data to better support AI systems.


# Features
- Analytics dashboard
Evaluate model performance using metrics such as ARR (Accuracy Rate Ratio) per NSI and per model.
- Response inspection
Load and review all responses for a given prompt.

# Access & Limitations
Submitting new prompts is currently limited to CBS researchers only.
Public users can explore results and existing evaluations.

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SNStatComp_AI_stats_measurement&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=SNStatComp_AI_stats_measurement)

# Kubernetes / SSP Cloud commands

## Check pods

```bash
kubectl get pods -n user-diegoespinosa
kubectl get pods -n user-diegoespinosa -w
```

## Check services

```bash
kubectl get svc -n user-diegoespinosa
```

## Check ingress

```bash
kubectl get ingress -n user-diegoespinosa
```

## Check logs

```bash
kubectl logs -f deployment/ai-stats-backend -n user-diegoespinosa
kubectl logs --tail=200 deployment/ai-stats-backend -n user-diegoespinosa
```

## Restart deployment

```bash
kubectl rollout restart deployment ai-stats-backend -n user-diegoespinosa
```

## Deploy or update with Helm

```bash
helm upgrade --install ai-stats ./ai-stats-measurement-chart -n user-diegoespinosa
```

## Uninstall Helm release

```bash
helm uninstall ai-stats -n user-diegoespinosa
```

## Delete pod

```bash
kubectl delete pod ai-stats-postgres-0 -n user-diegoespinosa
```

## Delete database PVC 

```bash
kubectl delete pvc postgres-data-ai-stats-postgres-0 -n user-diegoespinosa
```

## Fully reset app and database

```bash
helm uninstall ai-stats -n user-diegoespinosa
kubectl delete pvc postgres-data-ai-stats-postgres-0 -n user-diegoespinosa
helm upgrade --install ai-stats ./ai-stats-measurement-chart -n user-diegoespinosa
```

## Check secrets

```bash
kubectl get secrets -n user-diegoespinosa
kubectl describe secret ai-stats-measurement-secrets -n user-diegoespinosa
```

## After pushing a new latest image

```bash
kubectl rollout restart deployment ai-stats-backend -n user-diegoespinosa
kubectl rollout restart deployment ai-stats-frontend -n user-diegoespinosa
```

## Pause
```bash
kubectl scale deployment ai-stats-backend --replicas=0 -n user-diegoespinosa
kubectl scale deployment ai-stats-frontend --replicas=0 -n user-diegoespinosa
kubectl scale statefulset ai-stats-postgres --replicas=0 -n user-diegoespinosa
```

## Resume
```bash
kubectl scale statefulset ai-stats-postgres --replicas=1 -n user-diegoespinosa
kubectl scale deployment ai-stats-backend --replicas=1 -n user-diegoespinosa
kubectl scale deployment ai-stats-frontend --replicas=1 -n user-diegoespinosa
```
