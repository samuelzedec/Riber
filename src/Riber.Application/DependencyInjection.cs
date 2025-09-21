﻿using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Riber.Application.Behaviors;

namespace Riber.Application;

/// <summary>
/// Fornece métodos e configurações para configurar o container de injeção de dependência da camada de Application.
/// Esta classe tem como objetivo simplificar o processo de registro de serviços de casos de uso,
/// validações e regras de negócio necessárias para o funcionamento da aplicação.
/// </summary>
public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.NotificationPublisherType = typeof(Mediator.TaskWhenAllPublisher);
            options.PipelineBehaviors = [
                typeof(ValidationBehavior<,>),
                typeof(LoggingBehavior<,>)
            ];
        });

        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly,
            includeInternalTypes: true
        );
    }
}