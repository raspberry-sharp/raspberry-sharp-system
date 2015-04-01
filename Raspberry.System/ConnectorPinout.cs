namespace Raspberry
{
    /// <summary>
    /// The Raspberry Pi connector pinout revision.
    /// </summary>
    public enum ConnectorPinout
    {
        /// <summary>
        /// Connector pinout is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// The first revision, as of Model B rev1.
        /// </summary>
        Rev1,

        /// <summary>
        /// The second revision, as of Model B rev2.
        /// </summary>
        Rev2,

        /// <summary>
        /// The third revision, as of Model B+.
        /// </summary>
        Plus,
    }
}