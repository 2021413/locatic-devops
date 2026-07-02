namespace Locatic.Application.Services;

public interface ITableauDeBordService
{
    Task<TableauDeBordStats> ObtenirStatsAsync(DateOnly aujourdhui, CancellationToken cancellationToken = default);
}
