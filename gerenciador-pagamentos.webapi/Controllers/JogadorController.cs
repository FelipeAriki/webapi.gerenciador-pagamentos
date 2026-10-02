using gerenciador_pagamentos.webapi.Interface;
using gerenciador_pagamentos.webapi.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace gerenciador_pagamentos.webapi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JogadorController(IJogadorService jogadorService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PaginaJogadoresViewModel>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginaJogadoresViewModel>> ObterDadosJogadores(
        [FromQuery] ConsultaJogadoresViewModel consulta, CancellationToken cancellationToken) =>
        Ok(await jogadorService.ObterDadosJogadores(consulta, cancellationToken));

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType<JogadorViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JogadorViewModel>> ObterDadosJogador(int id, CancellationToken cancellationToken)
    {
        var jogador = await jogadorService.ObterDadosJogador(id, cancellationToken);
        return jogador is null ? RegistroNaoEncontrado() : Ok(jogador);
    }

    [HttpPost]
    [ProducesResponseType<JogadorViewModel>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JogadorViewModel>> CriarJogador([FromBody] SalvarJogadorViewModel dados, CancellationToken cancellationToken)
    {
        var jogador = await jogadorService.CriarJogador(dados, cancellationToken);
        return CreatedAtAction(nameof(ObterDadosJogador), new { id = jogador.Id }, jogador);
    }

    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType<JogadorViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JogadorViewModel>> AlterarJogador(int id, [FromBody] SalvarJogadorViewModel dados, CancellationToken cancellationToken)
    {
        var jogador = await jogadorService.AlterarJogador(id, dados, cancellationToken);
        return jogador is null ? RegistroNaoEncontrado() : Ok(jogador);
    }

    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExcluirJogador(int id, CancellationToken cancellationToken) =>
        await jogadorService.ExcluirJogador(id, cancellationToken) ? NoContent() : RegistroNaoEncontrado();

    private ObjectResult RegistroNaoEncontrado() => Problem(statusCode: StatusCodes.Status404NotFound,
        title: "Jogador não encontrado.", detail: "O jogador pode ter sido excluído. Atualize a listagem e tente novamente.");
}
