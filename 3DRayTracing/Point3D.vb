Public Class Point3D
    Private xCoord As Double
    Private yCoord As Double
    Private zCoord As Double

    Public Sub New(x As Double, y As Double, z As Double)
        xCoord = x
        yCoord = y
        zCoord = z
    End Sub

    Public Property x() As Double
        Get
            Return xCoord
        End Get
        Set(value As Double)
            xCoord = value
        End Set
    End Property
    Public Property y() As Double
        Get
            Return yCoord
        End Get
        Set(value As Double)
            yCoord = value
        End Set
    End Property
    Public Property z As Double
        Get
            Return zCoord
        End Get
        Set(value As Double)
            zCoord = value
        End Set
    End Property

    Public Shared Function addVector(r1 As Vector, i1 As Point3D) As Point3D
        Return New Point3D(i1.x + r1.myX, i1.y + r1.myY, i1.z + r1.myZ)
    End Function

    Public Overrides Function toString() As String
        Return "(" + CStr(Math.Round(xCoord, 4)) + "," + CStr(Math.Round(yCoord, 4)) + "," + CStr(Math.Round(zCoord, 4)) + ")"
        'Return "(" + CStr(xCoord) + "," + CStr(yCoord) + "," + CStr(zCoord) + ")"

    End Function

End Class
