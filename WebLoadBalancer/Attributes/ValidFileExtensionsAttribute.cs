using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ValidFileExtensionsAttribute : ValidationAttribute
{
    private string[] _allowedExtensions;
    private long _maxFileSize; 

    public ValidFileExtensionsAttribute(long maxFileSize, params string[] allowedExtensions)
    {
        _maxFileSize = maxFileSize;
        _allowedExtensions = allowedExtensions;
    }

    public override bool IsValid(object value)
    {
        if (value is IFormFile file)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!string.IsNullOrEmpty(fileExtension) && _allowedExtensions.Contains(fileExtension) && file.Length <= _maxFileSize)
            {
                return true;
            }
        }

        return false;
    }
}
