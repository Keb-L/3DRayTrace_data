Public Class Sphere
    Private origin As Point3D
    Private radius As Double

    Public Sub New(orig As Point3D, rad As Double)
        Me.origin = orig
        Me.radius = rad
    End Sub

    Public Property myOrigin()
        Get
            Return origin
        End Get
        Set(value)
            origin = value
        End Set
    End Property

    Public Property myRadius()
        Get
            Return radius
        End Get
        Set(value)
            radius = value
        End Set
    End Property

    Public Overrides Function ToString() As String
        Return "Center: " + origin.toString + " , Radius: " + CStr(radius)
    End Function
End Class
