! From http://www-h.eng.cam.ac.uk/help/mjg17/f90/mergsort.f90
MODULE Sort

CONTAINS

RECURSIVE SUBROUTINE Module_merge_sort (a, ascend)

  IMPLICIT NONE

  INTEGER, DIMENSION(:), INTENT(INOUT) :: a
  LOGICAL, INTENT(IN), OPTIONAL :: ascend
  LOGICAL :: up

  INTEGER low, high, mid

  ! If 'ascend' parameter is not specified, then default sort to ascending order
  IF (PRESENT(ascend)) THEN
    up = ascend
  ELSE
    up = .TRUE.
  ENDIF

  low=LBOUND(a,1)
  high=UBOUND(a,1)

  IF (low<high) THEN
    mid=(low+high)/2
    CALL Module_merge_sort(a(low:mid), up)
    CALL Module_merge_sort(a(mid+1:high), up)
    a(low:high) = Merge(a(low:mid), a(mid+1:high), up)
  END IF

END SUBROUTINE Module_merge_sort


FUNCTION Merge (a, b, up)

  INTEGER, DIMENSION(:), INTENT(INOUT) :: a, b
  INTEGER, DIMENSION(SIZE(a)+SIZE(b)) :: Merge
  LOGICAL, INTENT(IN) :: up

  INTEGER a_ptr, a_high, a_low
  INTEGER b_ptr, b_high, b_low
  INTEGER c_ptr

  LOGICAL condition

  a_low=LBOUND(a,1)
  a_high=UBOUND(a,1)
  b_low=LBOUND(b,1)
  b_high=UBOUND(b,1)

  a_ptr=a_low
  b_ptr=b_low
  c_ptr=1

  DO WHILE (a_ptr<=a_high .AND. b_ptr<=b_high)

    IF (up) THEN
      condition= (a(a_ptr) <= b(b_ptr))
    ELSE
      condition= (a(a_ptr) >= b(b_ptr))
    END IF

    IF (condition) THEN
      Merge(c_ptr)=a(a_ptr)
      a_ptr=a_ptr+1
    ELSE
      Merge(c_ptr)=b(b_ptr)
      b_ptr=b_ptr+1
    END IF

    c_ptr = c_ptr + 1

  END DO

  IF (a_ptr>a_high) THEN
    Merge(c_ptr:) = b(b_ptr:b_high)
  ELSE
    Merge(c_ptr:) = a(a_ptr:a_high)
  END IF

END FUNCTION Merge

END MODULE Sort




PROGRAM Merge_sort

  USE Sort
  INTEGER, DIMENSION(:), ALLOCATABLE :: array
  INTEGER i, n
  REAL r, time

  PRINT*, "Enter array size:"
  READ(*,*) n

  ALLOCATE( array(n) )

  DO i=1, n
    CALL RANDOM_NUMBER(r)
    array(i)=100 * r
  END DO

  PRINT '(20I3)', array(1:20)
  time = Second()
  CALL Module_merge_sort(array)
  PRINT '("[Sort time = ",F10.3," seconds ]")', Second() - time
  PRINT '(20I3)', array(1:20)
  time = Second()
  CALL Module_merge_sort(array, .FALSE.)
  PRINT '("[Sort time = ",F10.3," seconds ]")', Second() - time
  PRINT '(20I3)', array(1:20)


CONTAINS

REAL FUNCTION Second()

  IMPLICIT NONE

  INTEGER i, timer_count_rate, timer_count_max

  CALL SYSTEM_CLOCK( i, timer_count_rate, timer_count_max )
  Second = REAL(i) / timer_count_rate

END FUNCTION Second


END PROGRAM Merge_sort

