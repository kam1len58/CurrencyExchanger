using CurrencyExchanger.API.Exceptions;
using CurrencyExchanger.API.Models.Requests;
using CurrencyExchanger.API.Models.Responses;
using CurrencyExchanger.BLL.Services;
using CurrencyExchanger.BLL.Validators;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchanger.API.Controllers;

[ApiController]
[Route("currencies")]
public class CurrenciesController(CurrencyValidator currencyValidator, CurrencyService currencyService) : ControllerBase
{
    private readonly CurrencyValidator _currencyValidator = currencyValidator;
    private readonly CurrencyService _currencyService = currencyService;

    [HttpGet]
    public async Task<ActionResult> GetCurrencies()
    {
        var currencies = (await _currencyService.GetAllCurrenciesAsync())
            .Select(c => new CurrencyResponse
            {
                Id = c.Id,
                Name = c.FullName,
                Code = c.Code,
                Sign = c.Sign,
            })
            .ToList();

        return Ok(currencies);
    }

    [HttpPost]
    public async Task<ActionResult<CurrencyResponse>> AddCurrency([FromForm] CurrencyRequest currencyRequest)
    {
        var code = currencyRequest.Code;
        var name = currencyRequest.Name;
        var sign = currencyRequest.Sign;

        if (code is null || name is null || sign is null)
            throw new BadRequestException("Отсутствует нужное поле формы");

        if (await _currencyService.IsCurrencyExistsAsync(code))
            throw new ConflictException("Валюта с таким кодом уже существует в базе данных");

        _currencyValidator.ValidateCurrencyParameters(code, name, sign);
        var currency = await _currencyService.AddCurrencyAsync(name, code, sign);
        var currencyResponse = new CurrencyResponse
        {
            Id = currency.Id,
            Name = currency.FullName,
            Code = currency.Code,
            Sign = currency.Sign
        };

        return CreatedAtAction(nameof(GetCurrencies), new { code = currencyResponse.Code }, currencyResponse);
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<CurrencyResponse>> GetCurrency(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new BadRequestException("Код валюты отсутствует в адресе");

        var currency = await _currencyService.GetCurrencyByCodeAsync(code);

        if (currency is null)
            throw new NotFoundException("Валюта не найдена");

        var currencyResponse = new CurrencyResponse
        {
            Id = currency.Id,
            Name = currency.FullName,
            Code = currency.Code,
            Sign = currency.Sign
        };

        return Ok(currencyResponse);
    }
}
