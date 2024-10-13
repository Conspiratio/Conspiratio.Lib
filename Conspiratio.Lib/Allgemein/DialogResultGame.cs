namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Unser eigener enum, als Vorlage diente `System.Windows.Forms.DialogResult´.
    /// Da wir hier aber keine Abhängigkeit auf `System.Windows.Forms´ haben können und wollen, gibt es diesen eigenen Typ.
    /// </summary>
    public enum DialogResultGame
    {
        /// <summary>Nothing Das Dialogfeld zurück. Dies bedeutet, dass das modale Dialogfeld weiterhin ausgeführt wird.</summary>
        None,
        /// <summary>Das Dialogfeld Rückgabewert ist OK (üblicherweise von der Schaltfläche OK gesendet).</summary>
        OK,
        /// <summary>Das Dialogfeld Rückgabewert ist Cancel (in der Regel von der Schaltfläche Abbrechen gesendet).</summary>
        Cancel,
        /// <summary>Das Dialogfeld Rückgabewert ist Abort (in der Regel von der Schaltfläche Abbrechen gesendet).</summary>
        Abort,
        /// <summary>Das Dialogfeld Rückgabewert ist Retry (in der Regel von der Schaltfläche Wiederholen gesendet).</summary>
        Retry,
        /// <summary>Das Dialogfeld Rückgabewert ist Ignore (in der Regel von der Schaltfläche Ignorieren gesendet).</summary>
        Ignore,
        /// <summary>Das Dialogfeld Rückgabewert ist Yes (in der Regel von der Schaltfläche Ja gesendet).</summary>
        Yes,
        /// <summary>Das Dialogfeld Rückgabewert ist No (in der Regel von der Schaltfläche Nein gesendet).</summary>
        No
    }
}
