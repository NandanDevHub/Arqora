using Arqora.Application.Features.Quotations.Commands;
using Arqora.Application.Features.Quotations.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arqora.API.Controllers;

[Authorize]
public class QuotationsController : BaseApiController
{
    /// <summary>POST /api/quotations/generate — generate a full quotation for a project.</summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate(GenerateQuotationCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>GET /api/quotations/{id} — get full quotation detail with line items.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetQuotationByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>GET /api/quotations/project/{projectId} — list quotations for a project.</summary>
    [HttpGet("project/{projectId:guid}")]
    public async Task<IActionResult> GetByProject(Guid projectId)
    {
        var result = await Mediator.Send(new GetQuotationsByProjectQuery { ProjectId = projectId });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>GET /api/quotations/{id}/summary — category and room breakdown.</summary>
    [HttpGet("{id:guid}/summary")]
    public async Task<IActionResult> GetSummary(Guid id)
    {
        var result = await Mediator.Send(new GetQuotationSummaryQuery { QuotationId = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>GET /api/quotations/{id}/pdf — download quotation as PDF.</summary>
    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> GetPdf(Guid id)
    {
        var result = await Mediator.Send(new GetQuotationPdfQuery { QuotationId = id });
        return result.IsSuccess
            ? File(result.Value!, "application/pdf", $"Quotation-{id}.pdf")
            : NotFound(result.Error);
    }

    /// <summary>PUT /api/quotations/{id}/status — update quotation status (Draft, Sent, Accepted, etc.).</summary>
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateQuotationStatusCommand command)
    {
        command.Id = id;
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
