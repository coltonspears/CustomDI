# Dependency Injection Framework Requirements

## Overview
This document outlines the requirements for a custom dependency injection framework for .NET Framework 4.8 WPF applications. The framework will be developed without relying on NuGet packages, making it suitable for environments with restricted package management.

## Core Requirements

### 1. Service Registration
- Interface-based registration (register interface to implementation)
- Concrete type registration
- Instance registration (pre-created objects)
- Factory-based registration (custom creation logic)

### 2. Service Resolution
- Constructor injection
- Support for resolving complex dependency graphs
- Circular dependency detection
- Missing dependency handling
- Support for optional dependencies

### 3. Lifecycle Management
- Singleton (one instance for the entire application)
- Transient (new instance each time)
- Scoped (one instance per scope)
- Custom lifecycles

### 4. WPF-Specific Features
- ViewModel locator pattern support
- Integration with XAML
- Support for design-time data

## Additional Features

### 1. Property Injection
- Attribute-based property injection
- Explicit property injection configuration

### 2. Factory Support
- Func<T> factory resolution
- Factory method registration
- Lazy<T> support for deferred instantiation

### 3. Configuration Helpers
- Fluent API for registration
- Convention-based registration
- Assembly scanning capabilities

### 4. Advanced Features
- Conditional registration
- Named/keyed services
- Collection resolution (IEnumerable<T>)
- Child containers/scopes
- Interception/decorators

## Non-Functional Requirements
- Minimal memory footprint
- Fast resolution performance
- Thread safety
- Clear error messages
- Comprehensive documentation
- Example usage patterns
