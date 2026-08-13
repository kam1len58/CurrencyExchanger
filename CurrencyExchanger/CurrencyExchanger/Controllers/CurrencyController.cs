using CurrencyExchanger.Dao;
using CurrencyExchanger.Domain;
using CurrencyExchanger.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace CurrencyExchanger.Controllers;

[ApiController]
[Route("currency/{code?}")]
public class CurrencyController : ControllerBase
{
    private readonly DBConnectionProvider _connectionProvider;

    public CurrencyController(DBConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    [HttpGet]
    public ActionResult GetCurrency(string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(new { message = "Код валюты отсутствует в адресе" });

            var currencyDao = new CurrencyDao(_connectionProvider);
            var currency = currencyDao.GetCurrencyByCode(code);

            if (currency is null)
                return NotFound(new { message = "Валюта не найдена" });

            return Ok(currency);
        }
        catch (SqliteException)
        {
            return StatusCode(500, new { message = "База данных недоступна" });
        } 
    }
}
