using BolsoEmDia.Front.Components;
using BolsoEmDia.Front.Extensions;
using BolsoEmDia.Front.Models.Validadores;
using BolsoEmDia.Front.Services.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

// O front autentica por cookie; o JWT devolvido pela Api fica guardado dentro dele
// (StoreTokens) e é lido de lá pelo JwtAuthorizationHandler a cada chamada.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;
    });

// Toda rota exige autenticação por padrão — /login e /registrar são a exceção
// explícita ([AllowAnonymous]), em vez do inverso ([Authorize] em cada página nova).
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddServices(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Sem isso, o FallbackPolicy (autenticação obrigatória por padrão) também varre os
// endpoints de asset estático — anônimo em /login não consegue nem carregar o CSS.
app.MapStaticAssets().AllowAnonymous();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAutenticacaoEndpoints();

app.Run();
