using gerenciador_pagamentos.webapi.Interface;
using gerenciador_pagamentos.webapi.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gerenciador_pagamentos.webapi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JogadorController : ControllerBase
{
    private readonly IJogadorService _jogadorService;

    public JogadorController(IJogadorService jogadorService)
    {
        _jogadorService = jogadorService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<JogadorViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<JogadorViewModel>>> ObterDadosJogadores()
    {
        var jogadores = await _jogadorService.ObterDadosJogadores();
        return Ok(jogadores);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(JogadorViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JogadorViewModel>> ObterDadosJogador(int id)
    {
        var jogador = await _jogadorService.ObterDadosJogador(id);
        return jogador is null ? NotFound() : Ok(jogador);
    }
}
