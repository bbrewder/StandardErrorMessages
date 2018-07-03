using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace StandardErrorMessages
{
	/// <summary>
	/// Provides a standardized error response for the API.
	/// </summary>
	/// <remarks>
	/// Based on RFC 7807. http://www.rfc-editor.org/info/rfc7807
	/// </remarks>
	public class ErrorResponse
	{

		/// <summary>
		/// The content-type for the response.
		/// </summary>
		public const string ContentType = "application/problem+json";

		/// <summary>
		/// A URI reference [RFC3986] that identifies the 
		/// problem type.This specification encourages that, when 
		/// dereferenced, it provide human-readable documentation for the 
		/// problem type (e.g., using HTML [W3C.REC-html5-20141028]).
		/// </summary>
		public Uri Type { get; set; }

		/// <summary>
		/// A short, human-readable summary of the problem type. It SHOULD 
		/// NOT change from occurrence to occurrence of the problem, except 
		/// for purposes of localization
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// A human-readable explanation specific to this occurrence of the 
		/// problem.
		/// </summary>
		public string Detail { get; set; }
		
		#region Helper Methods

		private static Uri GetErrorUri(string errType)
		{
			return new System.Uri($"/Docs/Errors/{errType}", UriKind.Relative);
		}

		internal static ErrorResponse Exception(Exception ex)
		{
			var err = new ErrorResponse();
			var exType = ex.GetType();
			if (ex is ArgumentException)
			{
				err.Type = GetErrorUri("InvalidArgument");
				err.Title = "Invalid argument.";
			}
			else
			{
				err.Type = GetErrorUri(exType.FullName);
				err.Title = exType.FullName;
			}

			err.Detail = ex.Message;

			return err;
		}

		internal static ErrorResponse InvalidPath(string requestPath)
		{
			var err = new ErrorResponse();

			err.Type = GetErrorUri($"InvalidPath");
			err.Title = $"Invalid URI path.";
			err.Detail = $"The path '{requestPath}' is not valid. Please check the endpoint and try again.";

			return err;
		}

		internal static object NotFound(string docType, string details)
		{
			var err = new ErrorResponse();

			err.Type = GetErrorUri($"{docType}NotFound");
			err.Title = $"{docType} not found.";
			err.Detail = details;

			return err;
		}

		internal static ErrorResponse InvalidModel(ModelStateDictionary modelState)
		{
			var err = new ErrorResponse();

			err.Type = GetErrorUri("InvalidModel");
			err.Title = "Invalid model.";
			err.Detail = modelState.ToString();

			return err;
		}

		#endregion

	}
}