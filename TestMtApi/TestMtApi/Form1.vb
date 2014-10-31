Imports MtApi

Public Class Form1

    Private apiClient As MtApiClient

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        apiClient = New MtApiClient
        AddHandler apiClient.QuoteUpdated, AddressOf QuoteUpdatedHandler

    End Sub

    Sub QuoteUpdatedHandler(sender As Object, symbol As String, bid As Double, ask As Double)
        Dim quoteSrt As String
        quoteSrt = symbol + ": Bid = " + bid.ToString() + "; Ask = " + ask.ToString()

        ListBoxQuotesUpdate.Invoke(Sub()
                                       ListBoxQuotesUpdate.Items.Add(quoteSrt)
                                   End Sub)

        Console.WriteLine(quoteSrt)

    End Sub

    Private Sub btnConnect_Click(sender As System.Object, e As System.EventArgs) Handles btnConnect.Click
        apiClient.BeginConnect(8222)
    End Sub

    Private Sub btnDisconnect_Click(sender As System.Object, e As System.EventArgs) Handles btnDisconnect.Click
        apiClient.BeginDisconnect()
    End Sub
End Class
