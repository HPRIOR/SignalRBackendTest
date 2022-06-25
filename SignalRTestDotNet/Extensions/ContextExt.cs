using SignalRTestDotNet.DOAs;
using SignalRTestDotNet.GameContextNs;

namespace SignalRTestDotNet.Extensions;

public static class PlayerDAOExtensions
{
    public static Player AsPlayer(this PlayerDAO playerDAO) => new Player
    {
        Url = playerDAO.Url,
        Country = playerDAO.Country,
        SessionId = playerDAO.SessionId,
        PlayerId = playerDAO.PlayerId
    };

}
