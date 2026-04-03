# Architecture

## Overview

AFH.Common.Errors defines a shared error model and a set of adapters that integrate with service hosts. The model stays stable and adapter responsibilities are limited to mapping or transport specifics.

## Packages

- `AFH.Common.Errors`: shared abstractions and models.
- `AFH.Common.Errors.AzureFunctions`: response writing and Functions-specific mappings.
- `AFH.Common.Errors.ApplicationInsights`: telemetry mapping and enrichment.
- `AFH.Common.Errors.Email`: email notification adapter.
- `AFH.Common.Errors.EntityFramework`: persistence adapter for error records.

## Integration Pattern

Services own their exception mapping and context capture. The SDK provides a predictable flow:

1. Service-local exception mapper returns an `ExceptionMappingResult`.
2. The `ErrorRecordBuilder` creates a standard `ErrorRecord`.
3. A response builder writes a consistent response payload.
4. Optional adapters persist or notify based on service policy.

## What Stays Local

- Domain-specific exception mapping and error codes.
- Any tenant/user/operation metadata.
- Notification routing and escalation policies.
- Data retention and persistence decisions.

## What Stays Shared

- Error model, codes, and builder contracts.
- Adapter-specific infrastructure (Functions response writer, telemetry mappers, persistence writer).
