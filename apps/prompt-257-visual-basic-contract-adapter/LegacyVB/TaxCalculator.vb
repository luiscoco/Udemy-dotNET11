Imports System
Public NotInheritable Class TaxCalculator
    Private Sub New()
    End Sub
    Public Shared Function Total(amount As Decimal, rate As Decimal) As Decimal
        If amount < 0D Then Throw New ArgumentOutOfRangeException(NameOf(amount))
        If rate < 0D OrElse rate > 1D Then Throw New ArgumentOutOfRangeException(NameOf(rate))
        Return Decimal.Round(amount * (1D + rate), 2, MidpointRounding.AwayFromZero)
    End Function
End Class
