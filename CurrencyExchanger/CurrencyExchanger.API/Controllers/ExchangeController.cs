using CurrencyExchanger.API.Exceptions;
using CurrencyExchanger.API.Models.Requests;
using CurrencyExchanger.API.Models.Responses;
using CurrencyExchanger.BLL.Services;
using CurrencyExchanger.BLL.Validators;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchanger.API.Controllers;


[ApiController]
[Route("exchange")]
public class ExchangeController(
    ExchangeRateValidator exchangeRateValidator,
    CurrencyService currencyService,
    ExchangeService exchangeService
) : ControllerBase
{
    private readonly ExchangeRateValidator _exchangeRateValidator = exchangeRateValidator;
    private readonly CurrencyService _currencyService = currencyService;
    private readonly ExchangeService _exchangeService = exchangeService;


    [HttpGet]
    public async Task<ActionResult<ExchangeResponse>> Exchange([FromQuery] ExchangeRequest request)
    {
        var from = request.From;
        var to = request.To;
        var amount = request.Amount;

        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to) || amount is null)
            throw new BadRequestException("Отсутствует нужное поле формы");

        _exchangeRateValidator.ValidateParameters(from, to, amount);

        var baseCurrency = await _currencyService.GetCurrencyByCodeAsync(from);
        var targetCurrency = await _currencyService.GetCurrencyByCodeAsync(to);

        if (baseCurrency is null || targetCurrency is null)
            throw new NotFoundException("Валютная пара отсутствует в базе данных");

        var baseCurrencyId = baseCurrency.Id;
        var targetCurrencyId = targetCurrency.Id;
        var exchange = await _exchangeService.ExchangeAsync(baseCurrencyId, targetCurrencyId, amount);

        var rate = exchange.Rate;
        if (rate is null)
            throw new NotFoundException("Валютная пара отсутствует в базе данных");

        var convertedAmount = exchange.ConvertedAmount;
        var exchangeResponse = new ExchangeResponse
        {
            BaseCurrency = new CurrencyResponse
            {
                Id = baseCurrencyId,
                Code = baseCurrency.Code,
                Name = baseCurrency.FullName,
                Sign = baseCurrency.Sign
            },
            TargetCurrency = new CurrencyResponse
            {
                Id = targetCurrencyId,
                Code = targetCurrency.Code,
                Name = targetCurrency.FullName,
                Sign = targetCurrency.Sign
            },
            Rate = exchange.Rate,
            Amount = amount,
            ConvertedAmount = convertedAmount
        };

        return Ok(exchangeResponse);
    }
}
