#ifdef DEFINE_TEST_RECURISVE_ONCE_ONLY
#  ifndef __TEST_RECURSIVE_H
#    define __TEST_RECURSIVE_H
#    include "test-recursive.h"
#    warning "Included test-recursive.h"
#  endif
#else
#  include "test-recursive.h"
#  warning "Included test-recursive.h"
#endif
