namespace VaatcoBMS.Application;

public class ApiResponse<T> where T : class
{
	public int Status { get; set; }
	public string Message { get; set; }
	public T Data { get; set; }
	public object Error { get; set; }

	public ApiResponse(int status, string message, T data, object error)
	{
		Status = status;
		Message = message;
		Data = data;
		Error = error;
	}

	public ApiResponse(int status, T data)
	{
		Status = status;
		Message = "";
		Data = data;
	}

	public ApiResponse(int status)
	{
		Status = status;
		Message = "";
	}


	public ApiResponse(int status, string message, object error)
	{
		Status = status;
		Message = message;
		Error = error;
	}
}

public class ApiResponse
{
	public int Status { get; set; }
	public string Message { get; set; }
	public object Error { get; set; }
}
