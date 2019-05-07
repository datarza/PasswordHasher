How to safely store passwords in the database? Is the default ASP.NET Core implementation the good choice? For what is &lt;TUser> used in the default ASP.NET Core PasswordHasher class definition? 

The default password hasher for ASP.NET Core Identity uses PBKDF2 for password hashing that is not support all hashing algorithms.

## PasswordHasher notes

- Supports SHA1, SHA256, SHA384 and SHA512 hashing algorithms
- Automatically generated randomized salt
- Count of iterations is a variable parameter
- Default parameters are strong  enough
- Inbound byte[] comparator is a service and can be changed

This library contains source code that I found on the Internet. Unfortunately, this code had minor issue with array indices. I fixed it and adopted it for ASP.NET Core 2.2. I believe that this library can be also used for ASP.NET Core 2.0.

### Supported interface

```markdown
    public interface IPasswordHasher
    {		
        string HashPassword(string password);
        bool VerifyHashedPassword(string hashedPassword, string providedPassword);
    }
```

### Parameters can be specified in Startup.cs

```markdown
    public void ConfigureServices(IServiceCollection services)
    {
        // Configuring PasswordHasher
        services.Configure<PasswordHasherOptions>(options =>
        {
            options.HashAlgorithm = PasswordHasherAlgorithms.SHA1;
            options.SaltSize = 16;
            options.Iterations = 8192;
        });
    
        // Registering PasswordHasher
        services.AddPasswordHasher();
        
        services.AddMvc();
    }
```

### Using example

```markdown
    public class IndexModel : PageModel
    {
        private readonly IPasswordHasher hasher;
    
        public IndexModel(IPasswordHasher hasher)
        {
            this.hasher = hasher;
        }
        
        public void OnGet()
        {
            var password = "my password";
            var passwordHash = hasher.HashPassword(password);
            var passwordCheck = hasher.VerifyHashedPassword(passwordHash, password);
        }
    }
```

### Deep into hashing passwords

```markdown
    public string HashPassword(string password)
    {
        byte[] saltBuffer;
        byte[] hashBuffer;
        
        using (var keyDerivation = new Rfc2898DeriveBytes(password, options.SaltSize, options.Iterations, options.HashAlgorithmName))
        {
            saltBuffer = keyDerivation.Salt;
            hashBuffer = keyDerivation.GetBytes(options.HashSize);
        }
        
        byte[] result = new byte[options.HashSize + options.SaltSize];
        Buffer.BlockCopy(hashBuffer, 0, result, 0, options.HashSize);
        Buffer.BlockCopy(saltBuffer, 0, result, options.HashSize, options.SaltSize);
        return Convert.ToBase64String(result);
    }
```

### Deep into verifing passwords

```markdown
    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
    	byte[] hashedPasswordBytes = Convert.FromBase64String(hashedPassword);
    	if (hashedPasswordBytes.Length != options.HashSize + options.SaltSize)
    	{
    		return false;
    	}
    
    	byte[] _hashBytes = new byte[options.HashSize];
    	Buffer.BlockCopy(hashedPasswordBytes, 0, _hashBytes, 0, options.HashSize);
    	byte[] _saltBytes = new byte[options.SaltSize];
    	Buffer.BlockCopy(hashedPasswordBytes, options.HashSize, _saltBytes, 0, options.SaltSize);
    
    	byte[] _providedHashBytes;
    	using (var keyDerivation = new Rfc2898DeriveBytes(providedPassword, _saltBytes, options.Iterations, options.HashAlgorithmName))
    	{
    		_providedHashBytes = keyDerivation.GetBytes(options.HashSize);
    	}
    
    	return comparer.Equals(_hashBytes, _providedHashBytes);
    }
```

### Support or Contact

Having questions? [Contact me](https://github.com/CanadianBeaver) and I will help you sort it out.
