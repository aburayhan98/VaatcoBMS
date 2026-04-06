
using Microsoft.Data.SqlClient;

namespace VaatcoBMS.Application.Configs;

public class DbConfig
{
	public string ConnectionString { get; set; }

	public SqlConnection Connection => new(ConnectionString);
}
