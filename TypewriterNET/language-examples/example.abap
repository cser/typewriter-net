* from http://ru.wikibooks.org/wiki/ABAP/BC/%D0%97%D0%B0%D0%B3%D1%80%D1%83%D0%B7%D0%BA%D0%B0_%D1%82%D1%80%D0%B0%D0%BD%D1%81%D0%BF%D0%BE%D1%80%D1%82%D0%BD%D1%8B%D1%85_%D0%B7%D0%B0%D0%BF%D1%80%D0%BE%D1%81%D0%BE%D0%B2
*=======================================*
* Автоматически сгенерированная         *
* Программа загрузки транспортных       *
* запросов                              *
*=======================================*
 REPORT  zbc_request_upload.
 TYPE-POOLS: abap, sabc, stms.
 CONSTANTS: gc_tp_fillclient LIKE stpa-command
 VALUE 'FILLCLIENT'.
 DATA:
   lt_request TYPE stms_tr_requests,
   lt_tp_maintain TYPE stms_tp_maintains.
 DATA:
   sl TYPE i,
   l_datafile(255) TYPE c,
   datafiles TYPE i,
   ret TYPE i,
   ans TYPE c.
 DATA:
   et_request_infos TYPE stms_wbo_requests,
   request_info TYPE stms_wbo_request,
   system TYPE tmscsys-sysnam,
   request LIKE e070-trkorr.
 DATA:
   folder TYPE string,
   retval LIKE TABLE OF ddshretval WITH HEADER LINE,
   fldvalue LIKE help_info-fldvalue,
   transdir TYPE text255,
   filename LIKE authb-filename,
   trfile(20) TYPE c.
 DATA:
   BEGIN OF datatab OCCURS 0,
	 buf(8192) TYPE c,
   END OF datatab.
 DATA: len TYPE i,
	   flen TYPE i.
 SELECTION-SCREEN COMMENT /1(79) comm_sel.
 PARAMETERS:
 p_cofile(255) TYPE c LOWER CASE OBLIGATORY.
 SELECTION-SCREEN SKIP.
 SELECTION-SCREEN BEGIN OF BLOCK b01
 WITH FRAME TITLE bl_title.
 PARAMETERS:
   p_addque AS CHECKBOX DEFAULT abap_true,
   p_tarcli LIKE tmsbuffer-tarcli
			DEFAULT sy-mandt
			MATCHCODE OBJECT h_t000,
	p_sepr OBLIGATORY.
 SELECTION-SCREEN END OF BLOCK b01.
 
 INITIALIZATION.
   bl_title = 'Параметры очереди импорта'(b01).
   comm_sel =
 'Имя должно начинаться на ''K''.'(001).
   IF sy-opsys = 'Windows NT'.
	 p_sepr = '\'.
   ELSE.
	 p_sepr = '/'.
   ENDIF.
 
 AT SELECTION-SCREEN ON VALUE-REQUEST FOR p_cofile.
   DATA:
	 file TYPE file_table,
	 rc TYPE i,
	 title TYPE string,
	 file_table TYPE filetable,
	 file_filter TYPE string
 VALUE 'CO-файлы (K*.*)|K*.*||'.
   title = 'Выберите CO-файл'(006).
   CALL METHOD cl_gui_frontend_services=>file_open_dialog
	 EXPORTING
	   window_title            = title
	   file_filter             = file_filter
	 CHANGING
	   file_table              = file_table
	   rc                      = rc
	 EXCEPTIONS
	   file_open_dialog_failed = 1
	   cntl_error              = 2
	   error_no_gui            = 3
	   not_supported_by_gui    = 4
	   OTHERS                  = 5.
   IF sy-subrc <> 0.
	 MESSAGE ID sy-msgid TYPE sy-msgty NUMBER sy-msgno
	 WITH sy-msgv1 sy-msgv2 sy-msgv3 sy-msgv4.
   ENDIF.
   READ TABLE file_table INTO file INDEX 1.
   p_cofile = file.
 
 AT SELECTION-SCREEN.
   DATA:
	file TYPE string.
   sl = STRLEN( p_cofile ).
 
   IF sl < 11.
	 MESSAGE e001(00)
	 WITH 'Неверный формат имени co-файла.'
 'Имя файла должно быть - KNNNNNNN.SSS'(009).
   ENDIF.
   sl = sl - 11.
   IF p_cofile+sl(1) NE 'K'.
	 MESSAGE e001(00)
	 WITH 'Неверный формат имени co-файла.'
 'Имя файла должно быть - KNNNNNNN.SSS'(009).
   ENDIF.
   sl = sl + 1.
   IF NOT p_cofile+sl(6) CO '0123456789'.
	 MESSAGE e001(00)
	 WITH 'Неверный формат имени co-файла.'
 'Имя файла должно быть - KNNNNNNN.SSS'(009).
   ENDIF.
   sl = sl + 6.
   IF p_cofile+sl(1) NE '.'.
	 MESSAGE e001(00)
	 WITH 'Неверный формат имени co-файла.'
 'Имя файла должно быть - KNNNNNNN.SSS'(009).
   ENDIF.
   sl = sl - 7.
   CLEAR datafiles.
   l_datafile = p_cofile.
   l_datafile+sl(1) = 'R'.
   file = l_datafile.
   IF cl_gui_frontend_services=>file_exist( file = file ) =
abap_true.
	 ADD 1 TO datafiles.
   ENDIF.
   l_datafile+sl(1) = 'D'.
   file = l_datafile.
   IF cl_gui_frontend_services=>file_exist( file = file ) =
abap_true.
	 ADD 1 TO datafiles.
   ENDIF.
   sl = sl + 8.
   request = p_cofile+sl(3).
   sl = sl - 8.
   CONCATENATE request p_cofile+sl(7) INTO request.
   TRANSLATE request TO UPPER CASE.
   IF datafiles = 0.
	 MESSAGE e398(00)
	 WITH 'Corresponding data-files of transport request'(010)
	 request
	 'not found.'(011).
   ELSE .
	 MESSAGE s398(00)
	 WITH datafiles
	 'data-files have been found for transport request'(012)
	 request.
   ENDIF.
 
 START-OF-SELECTION.
   DATA:
	   parameter TYPE spar,
	   parameters TYPE TABLE OF spar.
   CALL FUNCTION 'RSPO_R_SAPGPARAM'
	 EXPORTING
	   name   = 'DIR_TRANS'
	 IMPORTING
	   value  = transdir
	 EXCEPTIONS
	   error  = 1
	   OTHERS = 2.
   IF sy-subrc <> 0.
	 MESSAGE ID sy-msgid TYPE 'E' NUMBER sy-msgno
	 WITH sy-msgv1 sy-msgv2 sy-msgv3 sy-msgv4.
   ENDIF.
   filename = p_cofile+sl(11).
   TRANSLATE filename TO UPPER CASE.
   CONCATENATE transdir 'cofiles' filename
	 INTO filename
   SEPARATED BY p_sepr.
   OPEN DATASET filename FOR INPUT IN BINARY MODE.
   ret = sy-subrc.
   CLOSE DATASET filename.
   trfile = p_cofile+sl(11).
   TRANSLATE trfile TO UPPER CASE.
   PERFORM copy_file USING 'cofiles' trfile p_cofile.
   trfile(1) = 'R'.
   l_datafile+sl(1) = 'R'.
   PERFORM copy_file USING 'data' trfile l_datafile.
   IF datafiles > 1.
	 trfile(1) = 'D'.
	 l_datafile+sl(1) = 'D'.
	 PERFORM copy_file USING 'data' trfile l_datafile.
   ENDIF.
   IF p_addque = abap_true.
	 system = sy-sysid.
	 DO 1 TIMES.
* check authority to add request to the import queue
	   CALL FUNCTION 'TR_AUTHORITY_CHECK_ADMIN'
		 EXPORTING
		   iv_adminfunction = 'TADD'
		 EXCEPTIONS
		   e_no_authority   = 1
		   e_invalid_user   = 2
		   OTHERS           = 3.
	   IF sy-subrc <> 0.
		 MESSAGE ID sy-msgid TYPE sy-msgty NUMBER sy-msgno
		 WITH sy-msgv1 sy-msgv2 sy-msgv3 sy-msgv4.
		 EXIT.
	   ENDIF.
	   DATA ls_exception LIKE stmscalert.
	   CALL FUNCTION 'TMS_MGR_FORWARD_TR_REQUEST'
		 EXPORTING
		   iv_request      = request
		   iv_target       = system
		   iv_tarcli       = p_tarcli
		   iv_import_again = abap_true
		   iv_monitor      = abap_true
		   iv_verbose      = abap_true
		 IMPORTING
		   es_exception    = ls_exception
		 EXCEPTIONS
		   OTHERS          = 99.
	   CHECK sy-subrc = 0.
	   CALL FUNCTION 'TMS_MGR_READ_TRANSPORT_REQUEST'
		 EXPORTING
		   iv_request                 = request
		   iv_target_system           = system
		 IMPORTING
		   et_request_infos           = et_request_infos
		 EXCEPTIONS
		   read_config_failed         = 1
		   table_of_requests_is_empty = 2
		   system_not_available       = 3
		   OTHERS                     = 4.
	   CLEAR request_info.
	   READ TABLE et_request_infos INTO request_info INDEX 1.
	   IF request_info-e070-korrdev = 'CUST'
	   AND NOT p_tarcli IS INITIAL.
		 CALL FUNCTION 'TMS_MGR_MAINTAIN_TR_QUEUE'
		   EXPORTING
			 iv_command                 = gc_tp_fillclient
			 iv_system                  = system
			 iv_request                 = request
			 iv_tarcli                  = p_tarcli
			 iv_monitor                 = abap_true
			 iv_verbose                 = abap_true
		   IMPORTING
			 et_tp_maintains            = lt_tp_maintain
		   EXCEPTIONS
			 read_config_failed         = 1
			 table_of_requests_is_empty = 2
			 OTHERS                     = 3.
		 IF sy-subrc <> 0.
		   MESSAGE ID sy-msgid TYPE sy-msgty NUMBER sy-msgno
		   WITH sy-msgv1 sy-msgv2 sy-msgv3 sy-msgv4.
		   EXIT.
		 ENDIF.
	   ENDIF.
* check authority to start request import
	   CALL FUNCTION 'TR_AUTHORITY_CHECK_ADMIN'
		 EXPORTING
		   iv_adminfunction = 'IMPS'
		 EXCEPTIONS
		   e_no_authority   = 1
		   e_invalid_user   = 2
		   OTHERS           = 3.
 
	   IF sy-subrc <> 0.
		 MESSAGE ID sy-msgid TYPE sy-msgty NUMBER sy-msgno
		 WITH sy-msgv1 sy-msgv2 sy-msgv3 sy-msgv4.
		 EXIT.
	   ENDIF.
	   CALL FUNCTION 'TMS_MGR_IMPORT_TR_REQUEST'
		 EXPORTING
		   iv_system                  = system
		   iv_request                 = request
		   iv_client                  = p_tarcli
		 EXCEPTIONS
		   read_config_failed         = 1
		   table_of_requests_is_empty = 2
		   OTHERS                     = 3.
	 ENDDO.
   ENDIF.
*&-------------------------------------------*
*& form copy_file
*&-------------------------------------------*
* text
*-----------------------------------------*
* -->subdir text
* -->fname text
* -->source_filetext
*-----------------------------------------*
 FORM copy_file USING subdir fname source_file.
   DATA: l_filename TYPE string.
   DATA: lv_file_appl LIKE rcgfiletr-ftappl.
   l_filename = source_file.
   CONCATENATE transdir subdir fname
	 INTO filename
	 SEPARATED BY p_sepr.
   REFRESH datatab.
   lv_file_appl = filename.
   CALL FUNCTION 'C13Z_FILE_UPLOAD_BINARY'
	 EXPORTING
	   i_file_front_end   = l_filename
	   i_file_appl        = lv_file_appl
	   i_file_overwrite   = abap_true
	 EXCEPTIONS
	   fe_file_not_exists = 1
	   fe_file_read_error = 2
	   ap_no_authority    = 3
	   ap_file_open_error = 4
	   ap_file_exists     = 5
	   OTHERS             = 6.
   IF sy-subrc <> 0.
	 WRITE: / 'Файл'(005), trfile, ' ошибка загрузки'(007).
	 WRITE: / 'Проверьте ошибки в STMS'.
   ELSE.
	 WRITE: / 'Файл'(005), trfile, ' успешно загружен'(007).
	 WRITE: / 'Проверьте статус импорта в STMS'.
   ENDIF.
 ENDFORM. "copy_file
