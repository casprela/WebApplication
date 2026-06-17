Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports System.Text
Imports System.Web.Script.Serialization
Imports Microsoft.IdentityModel.Tokens

Public Class ValidadorJWT

  Private Const SECRET_KEY As String = "clave-secreta-minimo-32-caracteres!!"

  Public Shared Function ValidarPermiso(accion As String) As ResultadoValidacion
    Dim cookie = HttpContext.Current.Request.Cookies("auth_token")
    If cookie Is Nothing Then
      Return New ResultadoValidacion With {.EsValido = False, .Mensaje = "No autenticado, debe iniciar sesión"}
    End If

    Dim token = cookie.Value
    Dim handler As New JwtSecurityTokenHandler()
    Dim key = New SymmetricSecurityKey(Encoding.UTF8.GetBytes(SECRET_KEY))

    Dim validationParams As New TokenValidationParameters With {
        .ValidateIssuer = True,
        .ValidateAudience = True,
        .ValidateLifetime = True,
        .ValidateIssuerSigningKey = True,
        .ValidIssuer = "ApiLogin",
        .ValidAudience = "ReactApp",
        .IssuerSigningKey = key,
        .ClockSkew = TimeSpan.Zero
    }

    Dim principal As ClaimsPrincipal
    Try
      principal = handler.ValidateToken(token, validationParams, Nothing)
    Catch ex As SecurityTokenExpiredException
      Return New ResultadoValidacion With {.EsValido = False, .Mensaje = "La sesión expiró, vuelva a iniciar sesión"}
    Catch ex As Exception
      Return New ResultadoValidacion With {.EsValido = False, .Mensaje = "Token inválido"}
    End Try

    Dim permisosClaim = principal.FindFirst("permisos")
    If permisosClaim Is Nothing Then
      Return New ResultadoValidacion With {.EsValido = False, .Mensaje = "Token inválido"}
    End If

    Dim serializer As New JavaScriptSerializer()
    Dim permisos = serializer.Deserialize(Of Permisos)(permisosClaim.Value)

    Dim tienePermiso As Boolean
    Select Case accion
      Case "Get" : tienePermiso = permisos.Get
      Case "Add" : tienePermiso = permisos.Add
      Case "Update" : tienePermiso = permisos.Update
      Case "Delete" : tienePermiso = permisos.Delete
      Case Else : tienePermiso = False
    End Select

    If Not tienePermiso Then
      Return New ResultadoValidacion With {.EsValido = False, .Mensaje = "No tiene permisos para realizar esta acción"}
    End If

    Return New ResultadoValidacion With {.EsValido = True, .Mensaje = ""}
  End Function

End Class

Public Class ResultadoValidacion
  Public Property EsValido As Boolean
  Public Property Mensaje As String
End Class

Public Class Permisos
  Public Property [Get] As Boolean
  Public Property Add As Boolean
  Public Property Update As Boolean
  Public Property Delete As Boolean
End Class
