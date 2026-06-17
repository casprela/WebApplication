Imports System.Web.Services
Imports System.Web.Script.Serialization

<WebService(Namespace:="http://tempuri.org/")>
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<System.Web.Script.Services.ScriptService>
Public Class wsUsuarios
  Inherits System.Web.Services.WebService

  ' Lista ficticia en memoria
  Private Shared usuarios As New List(Of Usuario) From {
      New Usuario With {.Id = 1, .Nombre = "Carlos", .Email = "carlos@mail.com", .Activo = True},
      New Usuario With {.Id = 2, .Nombre = "Maria", .Email = "maria@mail.com", .Activo = True},
      New Usuario With {.Id = 3, .Nombre = "Juan", .Email = "juan@mail.com", .Activo = False}
  }

  <WebMethod>
  <Script.Services.ScriptMethod(ResponseFormat:=Script.Services.ResponseFormat.Json, UseHttpGet:=True)>
  Public Function GetAll() As String
    Dim serializer As New JavaScriptSerializer()
    Return serializer.Serialize(usuarios)
  End Function

  <WebMethod>
  <Script.Services.ScriptMethod(ResponseFormat:=Script.Services.ResponseFormat.Json, UseHttpGet:=True)>
  Public Function GetById(id As Integer) As String
    Dim serializer As New JavaScriptSerializer()
    Dim usuario = usuarios.FirstOrDefault(Function(u) u.Id = id)

    If usuario Is Nothing Then
      Return serializer.Serialize(New With {.success = False, .message = "Usuario no encontrado"})
    End If

    Return serializer.Serialize(usuario)
  End Function

  <WebMethod>
  <Script.Services.ScriptMethod(ResponseFormat:=Script.Services.ResponseFormat.Json, UseHttpGet:=True)>
  Public Function Filter(nombre As String, email As String, activo As String) As String
    Dim serializer As New JavaScriptSerializer()
    Dim resultado = usuarios.AsQueryable()

    If Not String.IsNullOrEmpty(nombre) Then
      resultado = resultado.Where(Function(u) u.Nombre.ToLower().Contains(nombre.ToLower()))
    End If

    If Not String.IsNullOrEmpty(email) Then
      resultado = resultado.Where(Function(u) u.Email.ToLower().Contains(email.ToLower()))
    End If

    If Not String.IsNullOrEmpty(activo) Then
      Dim activoBool As Boolean
      If Boolean.TryParse(activo, activoBool) Then
        resultado = resultado.Where(Function(u) u.Activo = activoBool)
      End If
    End If

    Return serializer.Serialize(resultado.ToList())
  End Function

  <WebMethod>
  <Script.Services.ScriptMethod(ResponseFormat:=Script.Services.ResponseFormat.Json, UseHttpGet:=True)>
  Public Function Add(nombre As String, email As String, activo As String) As String
    Dim serializer As New JavaScriptSerializer()


    Dim validacion = ValidadorJWT.ValidarPermiso("Add")
    If Not validacion.EsValido Then
      Return serializer.Serialize(New With {.success = False, .message = validacion.Mensaje})
    End If

    If String.IsNullOrEmpty(nombre) OrElse String.IsNullOrEmpty(email) Then
      Return serializer.Serialize(New With {.success = False, .message = "Nombre y email son requeridos"})
    End If

    If String.IsNullOrEmpty(nombre) OrElse String.IsNullOrEmpty(email) Then
      Return serializer.Serialize(New With {.success = False, .message = "Nombre y email son requeridos"})
    End If

    Dim emailExiste = usuarios.Any(Function(u) u.Email.ToLower() = email.ToLower())
    If emailExiste Then
      Return serializer.Serialize(New With {.success = False, .message = "El email ya existe"})
    End If

    Dim activoBool As Boolean
    Boolean.TryParse(activo, activoBool)

    Dim nuevoId = usuarios.Max(Function(u) u.Id) + 1
    Dim nuevoUsuario As New Usuario With {
        .Id = nuevoId,
        .Nombre = nombre,
        .Email = email,
        .Activo = activoBool
    }

    usuarios.Add(nuevoUsuario)

    Return serializer.Serialize(New With {.success = True, .message = "Usuario agregado", .usuario = nuevoUsuario})
  End Function

  <WebMethod>
  <Script.Services.ScriptMethod(ResponseFormat:=Script.Services.ResponseFormat.Json, UseHttpGet:=True)>
  Public Function Update(id As Integer, nombre As String, email As String, activo As String) As String
    Dim serializer As New JavaScriptSerializer()

    Dim validacion = ValidadorJWT.ValidarPermiso("Update")
    If Not validacion.EsValido Then
      Return serializer.Serialize(New With {.success = False, .message = validacion.Mensaje})
    End If

    Dim usuario = usuarios.FirstOrDefault(Function(u) u.Id = id)
    If usuario Is Nothing Then
      Return serializer.Serialize(New With {.success = False, .message = "Usuario no encontrado"})
    End If

    If String.IsNullOrEmpty(nombre) OrElse String.IsNullOrEmpty(email) Then
      Return serializer.Serialize(New With {.success = False, .message = "Nombre y email son requeridos"})
    End If

    Dim emailExiste = usuarios.Any(Function(u) u.Email.ToLower() = email.ToLower() AndAlso u.Id <> id)
    If emailExiste Then
      Return serializer.Serialize(New With {.success = False, .message = "El email ya existe"})
    End If

    Dim activoBool As Boolean
    Boolean.TryParse(activo, activoBool)

    usuario.Nombre = nombre
    usuario.Email = email
    usuario.Activo = activoBool

    Return serializer.Serialize(New With {.success = True, .message = "Usuario actualizado", .usuario = usuario})
  End Function

  <WebMethod>
  <Script.Services.ScriptMethod(ResponseFormat:=Script.Services.ResponseFormat.Json, UseHttpGet:=True)>
  Public Function Delete(id As Integer) As String
    Dim serializer As New JavaScriptSerializer()

    Dim validacion = ValidadorJWT.ValidarPermiso("Delete")
    If Not validacion.EsValido Then
      Return serializer.Serialize(New With {.success = False, .message = validacion.Mensaje})
    End If

    Dim usuario = usuarios.FirstOrDefault(Function(u) u.Id = id)
    If usuario Is Nothing Then
      Return serializer.Serialize(New With {.success = False, .message = "Usuario no encontrado"})
    End If

    usuarios.Remove(usuario)

    Return serializer.Serialize(New With {.success = True, .message = "Usuario eliminado"})
  End Function

End Class

Public Class Usuario
  Public Property Id As Integer
  Public Property Nombre As String
  Public Property Email As String
  Public Property Activo As Boolean
End Class
