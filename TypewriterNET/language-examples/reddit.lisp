;;; From http://homepage.mac.com/svc/LispMovies/reddit.lisp.html

(in-package :kpax-user)

(defwebapp :reddit
  (:index 'reddit-home)
  (:static-root "static/")
  (:unsecure t))

(defvar *id-counter* 0)

(defclass reddit-link ()
  ((url :reader get-url :initarg :url :initform nil)
   (title :reader get-title :initarg :title :initform "")
   (id :reader get-id :initform (incf *id-counter*))
   (timestamp :reader get-timestamp :initform (get-universal-time))
   (points :accessor get-points :initform 0)))

(defvar *all-links* '())

(defun add-new-link (url title)
  (push (make-instance 'reddit-link :url url :title title) *all-links*))

(defun get-sorted-links (sort-by)
  (let ((links (sort (copy-list *all-links*) #'> :key sort-by)))
    (subseq links 0 (min (length links) 25))))

(defun get-link-with-id (id)
  (find id *all-links* :key #'get-id))

(defun render-link (request-response link)
  (with-slots (url title timestamp points id)
      link
    (html-part (out request-response)
      (:li 
       (:a :href url :title url (str title))
       (fmt "Posted ~a ago. ~d point~:p. " (s-utils:format-duration (max 1 (- (get-universal-time) timestamp))) points)
       (:a :href (dynamic-url request-response 'reddit-up :id id) :title "Vote this link up" "Up")
       (:a :href (dynamic-url request-response 'reddit-down :id id) :title "Vote this link down" "Down")))))

(defun reddit-home (request-response)
  (html-page (out request-response)
    (:html
     (:head 
      (:title "Reddit.lisp") 
      (:link :rel "stylesheet" :type "text/css" :href (static-url request-response :webapp "reddit.css")))
     (:body 
      (:h1 "Reddit.lisp") (:h3 "In less than 100 lines of elegant code")
      (:p 
       (:a :href (dynamic-url request-response nil) :title "Reload the Reddit.lisp Home page" "Refresh")
       (:a :href (dynamic-url request-response 'reddit-new-link) :title "Submit a new link" "New link"))
      (:h2 "Highest Ranking Links")
      (:ol
       (loop :for link :in (get-sorted-links #'get-points) :do
             (render-link request-response link)))
      (:h2 "Lastest Links")
      (:ol
       (loop :for link :in (get-sorted-links #'get-timestamp) :do
             (render-link request-response link)))))))

(defun reddit-new-link (request-response &optional message)
  (html-page (out request-response)
    (:html
     (:head 
      (:title "Reddit.lisp - Submit a new link") 
      (:link :rel "stylesheet"  :type "text/css" :href (static-url request-response :webapp "reddit.css")))
     (:body 
      (:h1 "Reddit.lisp") (:h3 "Submit a new link")
      (when message (htm (:p (str message))))
      (:form :action (dynamic-url request-response 'reddit-submit-new-link) :method "post"
       (:input :type "text" :name "url" :value "http://" :size 48 :title "The URL of the new link")
       (:input :type "text" :name "title" :value "Title" :size 48 :title "The title of the new link")
       (:input :type "submit" :value "I Read It !"))
      (:p (:a :href (dynamic-url request-response nil) :title "Back to the Reddit.lisp Home page" "Home"))))))

(defun is-valid-url (url)
  (ignore-errors
    (multiple-value-bind (contents code)
        (s-http-client:do-http-request url)
      (and (stringp contents) (not (zerop (length contents))) (= 200 code)))))

(defun reddit-submit-new-link (request-response)
  (let ((url (get-request-parameter-value request-response "url"))
        (title (get-request-parameter-value request-response "title")))
    (cond ((or (null url) (equal url "") (equal url "http://")) 
           (reddit-new-link request-response "URL missing"))
          ((or (null title) (equal title "") (equal title "Title")) 
           (reddit-new-link request-response "Title missing"))
          ((is-valid-url url) 
           (add-new-link url title)
           (redirect-to request-response 'reddit-home))
          (t (reddit-new-link request-response "URL is not valid")))))

(defun reddit-up (request-response &optional (delta +1))
  (let* ((id (s-utils:parse-integer-safely (get-request-parameter-value request-response "id")))
         (link (find id *all-links* :key #'get-id)))
    (when link (incf (get-points link) delta))
    (redirect-to request-response 'reddit-home)))

(defun reddit-down (request-response)
  (reddit-up request-response -1))

