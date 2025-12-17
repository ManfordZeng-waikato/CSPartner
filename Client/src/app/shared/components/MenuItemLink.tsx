import React from "react";
import { MenuItem } from "@mui/material";
import { NavLink, useLocation } from "react-router";

type MatchMode = "exact" | "startsWith";

interface MenuItemLinkProps {
  children: React.ReactNode;
  to: string;
  matchMode?: MatchMode;
  exclude?: string[];
}

export default function MenuItemLink({
  children,
  to,
  matchMode = "exact",
  exclude = []
}: MenuItemLinkProps) {
  const { pathname } = useLocation();

  const baseActive =
    matchMode === "startsWith" ? pathname.startsWith(to) : pathname === to;

  const isExcluded = exclude.some((path) =>
    path === "/" ? pathname === "/" : pathname.startsWith(path)
  );

  const isActive = baseActive && !isExcluded;

  return (
    <MenuItem
      component={NavLink}
      to={to}
      selected={isActive}
      sx={{
        fontSize: "1.2rem",
        textTransform: "uppercase",
        fontWeight: "bold",
        color: isActive ? "common.white" : "inherit",
        "&.Mui-selected": {
          color: "common.white",
          backgroundColor: "rgba(255,255,255,0.16)"
        },
        "&:hover": {
          backgroundColor: "rgba(255,255,255,0.08)"
        }
      }}
    >
      {children}
    </MenuItem>
  );
}
