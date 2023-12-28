import { Avatar, Box, Container, Grid, Paper, TextField, Typography } from "@mui/material";
import LockOutlinedIcon from "@mui/icons-material/LockOutlined";
import { Link, useNavigate } from "react-router-dom";
import { FieldValues, useForm } from "react-hook-form";
import LoadingButton from "@mui/lab/LoadingButton";
import { useAppDispatch } from "../../app/store/configureStore";
import { signInUser } from "./accountSlice";

export default function Login() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const {
    register,
    handleSubmit,
    formState: { isSubmitting, errors, isValid },
  } = useForm({ mode: "onTouched" });

  async function submitForm(data: FieldValues) {
    const object = await dispatch(signInUser(data));
    if (!object.type.includes("rejected")) {
      navigate("/catalog");
    }
  }

  return (
    <Container
      component={Paper}
      maxWidth="sm"
      sx={{ display: "flex", flexDirection: "column", alignItems: "center", p: 4 }}
    >
      <Avatar sx={{ m: 1, bgcolor: "secondary.main" }}>
        <LockOutlinedIcon />
      </Avatar>
      <Typography component="h1" variant="h5">
        Sign in
      </Typography>
      <Box component="form" onSubmit={handleSubmit(submitForm)} noValidate sx={{ mt: 1 }}>
        <TextField
          margin="normal"
          fullWidth
          label="Username"
          autoFocus
          {...register("username", { required: "Username is required." })}
          error={!!errors.username}
          helperText={errors.username?.message as string}
        />
        <TextField
          margin="normal"
          fullWidth
          label="Password"
          type="password"
          {...register("password", { required: "Password is required." })}
          error={!!errors.password}
          helperText={errors.password?.message as string}
        />
        <LoadingButton
          disabled={!isValid}
          loading={isSubmitting}
          type="submit"
          fullWidth
          variant="contained"
          sx={{ mt: 3, mb: 2 }}
        >
          Sign in
        </LoadingButton>
        <Grid container>
          <Link to="/register">Don't have an account? Sign Up!</Link>
        </Grid>
      </Box>
    </Container>
  );
}
