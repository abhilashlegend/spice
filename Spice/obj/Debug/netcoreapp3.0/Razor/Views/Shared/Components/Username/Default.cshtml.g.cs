#pragma checksum "D:\Myprograms\dotnet\Spice\Spice\Views\Shared\Components\Username\Default.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "d1bf9a76487309e82edc166509cb47937d6f3a9a"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Shared_Components_Username_Default), @"mvc.1.0.view", @"/Views/Shared/Components/Username/Default.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "D:\Myprograms\dotnet\Spice\Spice\Views\_ViewImports.cshtml"
using Spice;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "D:\Myprograms\dotnet\Spice\Spice\Views\_ViewImports.cshtml"
using Spice.Models;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"d1bf9a76487309e82edc166509cb47937d6f3a9a", @"/Views/Shared/Components/Username/Default.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"b6d11972dba3ccf0da976b184cdef3b810ef5472", @"/Views/_ViewImports.cshtml")]
    public class Views_Shared_Components_Username_Default : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<Spice.Models.ApplicationUser>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\r\nHi ");
#nullable restore
#line 3 "D:\Myprograms\dotnet\Spice\Spice\Views\Shared\Components\Username\Default.cshtml"
Write(Model.Name);

#line default
#line hidden
#nullable disable
            WriteLiteral(" <i class=\"far fa-user\"></i>");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<Spice.Models.ApplicationUser> Html { get; private set; }
    }
}
#pragma warning restore 1591