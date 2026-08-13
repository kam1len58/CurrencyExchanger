using CurrencyExchanger.Dao;
using CurrencyExchanger.Domain;
using CurrencyExchanger.Domain.Validators;
using CurrencyExchanger.Dto;
using CurrencyExchanger.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace CurrencyExchanger.Controllers;

[ApiController]
[Route("currencies")]
public class CurrenciesController : ControllerBase
{
    private readonly DBConnectionProvider _connectionProvider;
    private readonly CurrencyValidator _currencyValidator;

    public CurrenciesController(DBConnectionProvider connectionProvider, CurrencyValidator currencyValidator)
    {
        _connectionProvider = connectionProvider;
        _currencyValidator = currencyValidator;
    }

    [HttpGet]
    public ActionResult GetCurrencies()
    {
        try
        {
            List<CurrencyDto> currencies = new CurrencyDao(_connectionProvider)
            .GetAllCurrencies()
            .Select(CurrencyMapper.ToDto)
            .ToList();
            return Ok(currencies);
        }
        catch (SqliteException)
        {
            return StatusCode(500, new { message = "База данных недоступна" });
        }
    }

    [HttpPost]
    public ActionResult AddCurrency([FromForm] string code, [FromForm] string name, [FromForm] string sign)
    {
        try
        {
            if(code is null || name is null || sign is null)
                return BadRequest(new { message = "Отсутствует нужное поле формы" });

            var currencyDao = new CurrencyDao(_connectionProvider);
            if (currencyDao.IsCurrencyExists(code))
                return Conflict(new { message = "Валюта с таким кодом уже существует в базе данных" });

            _currencyValidator.ValidateCurrencyParameters(code, name, sign);
            var currency = currencyDao.AddCurrency(name, code, sign);
            return CreatedAtAction(nameof(GetCurrencies), new { code = currency.Code }, currency);
        }
        catch (SqliteException)
        {
            return StatusCode(500, new { message = "База данных недоступна" });
        } 
    }
}
