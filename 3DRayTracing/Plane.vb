Public Class Plane
    Private a, b, c As Double
    Private intersection As Point3D
    Private x0, y0, z0 As Double
    Private normal As Vector
    Private equation As String

    Public Sub New(intersect As Point3D, normal As Vector)
        Me.intersection = intersect
        x0 = intersect.x
        y0 = intersect.y
        z0 = intersect.z
        Me.normal = normal
        Me.a = normal.myX
        Me.b = normal.myY
        Me.c = normal.myZ
        equation = CStr(Math.Round(a, 3)) + "(x - " + CStr(Math.Round(intersect.x, 3)) + ") + " + CStr(Math.Round(b, 3)) +
            "(y - " + CStr(Math.Round(intersect.y, 3)) + ") + " + CStr(Math.Round(c, 3)) + "(z - " + CStr(Math.Round(intersect.z, 3)) + ")"
    End Sub

    Public Function isOnPlane(pt As Point3D)
        Dim x, y, z As Double
        x = pt.x
        y = pt.y
        z = pt.z
        If Math.Round(a * (x - x0) + b * (y - y0) + c * (z - z0), 10) = 0 Then
            Return True
        Else
            ' Console.WriteLine(Math.Round(a * (x - x0) + b * (y - y0) + c * (z - z0)), 10)
            Return False
        End If
    End Function


    Public Function myEQ()
        Return equation
    End Function

    Public Property intersect()
        Get
            Return intersection
        End Get
        Set(value)
            intersection = value
        End Set
    End Property


    Public Property norm()
        Get
            Return normal
        End Get
        Set(value)
            normal = value
        End Set
    End Property
End Class
