# TSST

To jest fantastyczny projekt z TSST.

# Agent zarządzania

1. MS dostaje HELLO `<nazwa hosta>`  
  np. HELLO H1
2. MS odpowiada HELLO
3. Co 5s MS dostaje KEEPALIVE
4. (MS do LSR) ADD_RULE/REMOVE_RULE INC_PORT | INC_LABEL | OUT_PORT | OUT_LABEL | ADDITIONAL_LABEL
5. (MS do LER) ADD_BORDER_RULE/REMOVE_BORDER_RULE DESTINATION_IP | OUT_PORT | OUT_LABEL
