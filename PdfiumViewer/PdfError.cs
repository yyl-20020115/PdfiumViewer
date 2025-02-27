﻿#pragma warning disable 1591

namespace PdfiumViewer
{
    public enum PdfError : int
    {
        Success = (int)NativeMethods.FPDF_ERR.FPDF_ERR_SUCCESS,
        Unknown = (int)NativeMethods.FPDF_ERR.FPDF_ERR_UNKNOWN,
        CannotOpenFile = (int)NativeMethods.FPDF_ERR.FPDF_ERR_FILE,
        InvalidFormat = (int)NativeMethods.FPDF_ERR.FPDF_ERR_FORMAT,
        PasswordProtected = (int)NativeMethods.FPDF_ERR.FPDF_ERR_PASSWORD,
        UnsupportedSecurityScheme = (int)NativeMethods.FPDF_ERR.FPDF_ERR_SECURITY,
        PageNotFound = (int)NativeMethods.FPDF_ERR.FPDF_ERR_PAGE
    }
}
