using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SleepCommerce.Application.DTOs;
using SleepCommerce.Application.Interfaces;

namespace SleepCommerce.API.Controllers;

/// <summary>
/// Gerenciamento de produtos do catálogo.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Produtos")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    private readonly IValidator<ProductRequest> _validator;

    public ProductsController(IProductService service, IValidator<ProductRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    /// <summary>
    /// Lista produtos com paginação, filtro e ordenação.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductResponse>>> GetAll(
        [FromQuery] ProductQueryParameters parameters)
    {
        var result = await _service.GetAllAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Obtém um produto pelo seu ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id)
    {
        var product = await _service.GetByIdAsync(id);
        if (product is null) return NotFound();
        return Ok(product);
    }

    /// <summary>
    /// Cria um novo produto.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> Create([FromBody] ProductRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var product = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>
    /// Atualiza um produto existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> Update(Guid id, [FromBody] ProductRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var product = await _service.UpdateAsync(id, request);
        if (product is null) return NotFound();
        return Ok(product);
    }

    /// <summary>
    /// Remove um produto pelo ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
