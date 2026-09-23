const { sptField } = require("./../constants/splitter")

module.exports = function splitArrayToString(a) {
    var s, j;
    for (j = 0; j < a.length; j++) {
      if (j == 0) s = a[j];
      else s += sptField + a[j];
    }
    return s;
  }
  