import React from "react";
import { Link } from "@mui/material";
import { useUserProfile } from "../../../hooks/useUserProfile";
import { useNavigate } from "react-router";

interface UserNameLinkProps {
  userId: string;
  size?: 'small' | 'medium';
}

const UserNameLink: React.FC<UserNameLinkProps> = ({ userId, size = 'small' }) => {
  const { profile } = useUserProfile(userId);
  const navigate = useNavigate();
  const displayName = profile?.displayName || `User ${userId.substring(0, 8)}`;
  const fontSize = size === 'small' ? '0.75rem' : '0.875rem';

  const handleClick = (e: React.MouseEvent) => {
    e.preventDefault();
    navigate(`/user/${userId}`);
  };

  return (
    <Link
      component="button"
      onClick={handleClick}
      sx={{
        color: 'inherit',
        textDecoration: 'none',
        fontWeight: 500,
        fontSize: fontSize,
        cursor: 'pointer',
        '&:hover': {
          color: 'primary.main',
          textDecoration: 'underline'
        }
      }}
    >
      {displayName}
    </Link>
  );
};

export default UserNameLink;

