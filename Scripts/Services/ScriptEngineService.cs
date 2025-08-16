using Jint;
using Microsoft.Extensions.Logging;

namespace Leaderboard.Scripts;

public interface IScriptEngineService
{
    T ExecuteScript<T>(string scriptName, string functionName, params object[] parameters);
    void LoadScript(string scriptName);
    bool HasScript(string scriptName);
    bool HasFunction(string scriptName, string functionName); // Yeni method
}

public class ScriptEngineService : IScriptEngineService
{
    private readonly ILogger<ScriptEngineService> _logger;
    private readonly string _scriptsPath;
    private readonly Dictionary<string, bool> _loadedScripts;
    private Engine? _engine;
    
    public ScriptEngineService(ILogger<ScriptEngineService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _scriptsPath = Path.Combine(environment.ContentRootPath, "Scripts", "dist");
        _loadedScripts = new Dictionary<string, bool>();
        
        _logger.LogInformation("ScriptEngineService initialized. Scripts path: {ScriptsPath}", _scriptsPath);
        
        InitializeEngine();
    }
    
    private void InitializeEngine()
    {
        try
        {
            _engine = new Engine(cfg => cfg
                .AllowClr()
                .LimitMemory(4_000_000)
                .LimitRecursion(100)
            );
            
            _engine.SetValue("console", new
            {
                log = new Action<object>(msg => _logger.LogInformation("Script: {Message}", msg))
            });
            
            _engine.SetValue("globalThis", _engine.Global);
            
            _logger.LogInformation("Jint engine initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Jint engine");
            throw;
        }
    }
    
    public void LoadScript(string scriptName)
    {
        try
        {
            if (_loadedScripts.ContainsKey(scriptName))
            {
                _logger.LogInformation("Script {ScriptName} already loaded", scriptName);
                return;
            }
                
            var scriptPath = Path.Combine(_scriptsPath, $"{scriptName}.js");
            _logger.LogInformation("Loading script from: {ScriptPath}", scriptPath);
            
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"Script file not found: {scriptPath}");
            }
            
            var scriptContent = File.ReadAllText(scriptPath);
            _logger.LogInformation("Script content loaded, length: {Length}", scriptContent.Length);
            
            _engine!.Execute(scriptContent);
            _loadedScripts[scriptName] = true;
            
            _logger.LogInformation("Script {ScriptName} loaded successfully", scriptName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load script {ScriptName}", scriptName);
            throw;
        }
    }
    
    public bool HasScript(string scriptName)
    {
        try
        {
            var scriptPath = Path.Combine(_scriptsPath, $"{scriptName}.js");
            var exists = File.Exists(scriptPath);
            
            _logger.LogInformation("Checking if script exists: {ScriptName} -> {Exists}", scriptName, exists);
            
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if script exists: {ScriptName}", scriptName);
            return false;
        }
    }

    public bool HasFunction(string scriptName, string functionName)
    {
        try
        {
            // Önce script'i yükle
            LoadScript(scriptName);
            
            // Fonksiyon var mı kontrol et
            var hasFunction = _engine!.GetValue(functionName) != null;
            
            _logger.LogInformation("Checking if function exists: {ScriptName}.{FunctionName} -> {Exists}", 
                scriptName, functionName, hasFunction);
            
            return hasFunction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if function exists: {ScriptName}.{FunctionName}", 
                scriptName, functionName);
            return false;
        }
    }

    public T ExecuteScript<T>(string scriptName, string functionName, params object[] parameters)
    {
        try
        {
            _logger.LogInformation("Executing script {ScriptName}.{FunctionName} with {ParameterCount} parameters", 
                scriptName, functionName, parameters.Length);
            
            LoadScript(scriptName);
            
            var result = _engine!.Invoke(functionName, parameters);
            _logger.LogInformation("Script execution completed. Result: {Result}", result);
            
            return (T)Convert.ChangeType(result, typeof(T));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute script {ScriptName}.{FunctionName}", scriptName, functionName);
            throw;
        }
    }
}
