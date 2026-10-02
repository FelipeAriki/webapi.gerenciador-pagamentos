using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace gerenciador_pagamentos.webapi.Infrastructure;

public class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger, IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = 499;
            return true;
        }

        var validacao = exception is ValidationException;
        if (!validacao)
            logger.LogError(exception, "Falha na requisição {Method} {Path}. TraceId: {TraceId}",
                context.Request.Method, context.Request.Path, context.TraceIdentifier);

        context.Response.StatusCode = validacao ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = validacao ? "Dados inválidos." : "Não foi possível concluir a operação.",
                Detail = validacao ? exception.Message : "Tente novamente. Se o problema persistir, contate o responsável pelo sistema.",
                Extensions = { ["traceId"] = context.TraceIdentifier }
            }
        });
    }
}
