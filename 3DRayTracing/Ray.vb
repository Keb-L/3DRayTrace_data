Public Class Ray
    Private start As Point3D
    Private vect As Vector

    Public Sub New(origin As Point3D, change As Vector)
        start = origin
        vect = change

    End Sub

    Public Property myOrigin()
        Get
            Return start
        End Get
        Set(value)
            start = value
        End Set
    End Property
    Public Property myVector()
        Get
            Return vect
        End Get
        Set(value)
            vect = value
        End Set
    End Property

    Public Function axisIntersectionY(val As Double) As Point3D
        Dim delY As Double = val - myOrigin.y ' calculates difference
        'Dim dXZ As Double = myVector.myx / myVector.myZ
        'Dim dYZ As Double = myVector.myY / myVector.myZ

        Dim scale As Double = delY / myVector.myY ' calculates scaling


        Return New Point3D(myOrigin.x + myVector.myX * scale, val, myOrigin.z + myVector.myZ * scale)
        ' Return New Point3D(myOrigin.x + dXZ * delZ, myOrigin.y + dYZ * delZ, val)
    End Function


    Public Overrides Function ToString() As String
        Return "Starting point: " + start.toString + " , Change: " + vect.ToString
    End Function
End Class
