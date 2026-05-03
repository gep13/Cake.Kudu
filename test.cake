#reference "BuildArtifacts/temp/_PublishedLibraries/Cake.Kudu/net7.0/Cake.Kudu.dll"

// Self-contained exercise of the Cake.Kudu alias. Cake.Kudu's surface
// is a single CakePropertyAlias `Kudu` returning a KuduProvider built
// from the current process environment variables. We can exercise it
// locally by setting the WEBSITE_*, DEPLOYMENT_*, APPSETTING_* and
// SQLAZURECONNSTR_* environment variables BEFORE the property
// resolves (the alias is `Cache = true`, so the first read takes the
// snapshot for the whole script). The actual `Sync()` method calls
// the KuduSync.NET tool, which can't be exercised without the tool +
// a real source/target — so this script verifies provider properties
// only.

void AssertThat(bool condition, string message)
{
    if (!condition)
    {
        throw new Exception("Assertion failed: " + message);
    }
}

// Fake a Kudu host environment.
System.Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", "fake-site");
System.Environment.SetEnvironmentVariable("WEBSITE_HOSTNAME", "fake-site.azurewebsites.net");
System.Environment.SetEnvironmentVariable("WEBSITE_SKU", "Free");
System.Environment.SetEnvironmentVariable("REGION_NAME", "North Europe");
System.Environment.SetEnvironmentVariable("DEPLOYMENT_SOURCE", "/fake/source");
System.Environment.SetEnvironmentVariable("DEPLOYMENT_TARGET", "/fake/target");
System.Environment.SetEnvironmentVariable("DEPLOYMENT_TEMP", "/fake/temp");
System.Environment.SetEnvironmentVariable("APPSETTING_MyKey", "MyValue");
System.Environment.SetEnvironmentVariable("APPSETTING_OtherKey", "OtherValue");
System.Environment.SetEnvironmentVariable("SQLAZURECONNSTR_MyDb", "Server=tcp:fake;Database=Db");

// Capture the provider once — the alias is cached after the first
// read, which mirrors how the real recipe usage works.
var kudu = Kudu;

Task("Default")
    .IsDependentOn("Verify-IsRunningOnKudu")
    .IsDependentOn("Verify-WebSite")
    .IsDependentOn("Verify-Deployment")
    .IsDependentOn("Verify-AppSettings")
    .IsDependentOn("Verify-ConnectionStrings");

Task("Verify-IsRunningOnKudu")
    .Does(() =>
{
    AssertThat(kudu.IsRunningOnKudu,
        "IsRunningOnKudu should be true when WEBSITE_SITE_NAME is set");
    Information("IsRunningOnKudu OK");
});

Task("Verify-WebSite")
    .Does(() =>
{
    AssertThat(kudu.WebSite.Name == "fake-site",
        "WebSite.Name mismatch (got '" + (kudu.WebSite.Name ?? "null") + "')");
    AssertThat(kudu.WebSite.HostName == "fake-site.azurewebsites.net",
        "WebSite.HostName mismatch");
    AssertThat(kudu.WebSite.SKU == "Free", "WebSite.SKU mismatch");
    AssertThat(kudu.WebSite.Region == "North Europe", "WebSite.Region mismatch");
    Information("WebSite OK (Name={0}, SKU={1}, Region={2})",
        kudu.WebSite.Name, kudu.WebSite.SKU, kudu.WebSite.Region);
});

Task("Verify-Deployment")
    .Does(() =>
{
    AssertThat(kudu.Deployment.Source != null && kudu.Deployment.Source.FullPath.EndsWith("/source"),
        "Deployment.Source mismatch");
    AssertThat(kudu.Deployment.Target != null && kudu.Deployment.Target.FullPath.EndsWith("/target"),
        "Deployment.Target mismatch");
    AssertThat(kudu.Deployment.Temp != null && kudu.Deployment.Temp.FullPath.EndsWith("/temp"),
        "Deployment.Temp mismatch");
    Information("Deployment OK (Source={0}, Target={1}, Temp={2})",
        kudu.Deployment.Source, kudu.Deployment.Target, kudu.Deployment.Temp);
});

Task("Verify-AppSettings")
    .Does(() =>
{
    AssertThat(kudu.AppSettings.ContainsKey("MyKey"),
        "AppSettings should contain MyKey");
    AssertThat(kudu.AppSettings["MyKey"] == "MyValue",
        "AppSettings[MyKey] roundtrip mismatch");
    AssertThat(kudu.AppSettings.ContainsKey("OtherKey"),
        "AppSettings should contain OtherKey");
    AssertThat(kudu.AppSettings.Count >= 2,
        "AppSettings should have at least 2 entries");
    Information("AppSettings OK ({0} entries)", kudu.AppSettings.Count);
});

Task("Verify-ConnectionStrings")
    .Does(() =>
{
    AssertThat(kudu.ConnectionStrings.ContainsKey("MyDb"),
        "ConnectionStrings should contain MyDb");
    AssertThat(kudu.ConnectionStrings["MyDb"].Contains("Server=tcp:fake"),
        "ConnectionStrings[MyDb] roundtrip mismatch");
    Information("ConnectionStrings OK ({0} entries)", kudu.ConnectionStrings.Count);
});

RunTarget("Default");
