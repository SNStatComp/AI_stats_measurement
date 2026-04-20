{{- define "ai-stats-measurements-chart.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "ai-stats-measurements-chart.fullname" -}}
{{- if .Values.fullnameOverride -}}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- printf "%s-%s" .Release.Name (include "ai-stats-measurements-chart.name" .) | trunc 63 | trimSuffix "-" -}}
{{- end -}}
{{- end -}}

{{- define "ai-stats-measurements-chart.backendName" -}}
{{- printf "%s-backend" (include "ai-stats-measurements-chart.fullname" .) -}}
{{- end -}}

{{- define "ai-stats-measurements-chart.frontendName" -}}
{{- printf "%s-frontend" (include "ai-stats-measurements-chart.fullname" .) -}}
{{- end -}}

{{- define "ai-stats-measurements-chart.postgresName" -}}
{{- printf "%s-postgres" (include "ai-stats-measurements-chart.fullname" .) -}}
{{- end -}}