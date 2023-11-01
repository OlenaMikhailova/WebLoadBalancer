
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace WebLoadBalancer.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ValidFileExtensionsAttribute : ValidationAttribute
    {
        private string[] _allowedExtensions;

        public ValidFileExtensionsAttribute(params string[] allowedExtensions)
        {
            _allowedExtensions = allowedExtensions;
        }

        public override bool IsValid(object value)
        {
            if (value is IFormFile file)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!string.IsNullOrEmpty(fileExtension) && _allowedExtensions.Contains(fileExtension))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

