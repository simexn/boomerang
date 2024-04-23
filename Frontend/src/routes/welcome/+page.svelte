<script lang="ts">
    import { goto } from '$app/navigation';
    import { handleAccountRegister, handleAccountLogin, isLoggedIn, validateUsername, validateEmail, validatePassword, validateConfirmPassword } from '$lib/handlers/accountHandler';
    import { updateUserInfo } from '$lib/stores/userInfoStore';
    import { onMount } from 'svelte';
    import { fade } from 'svelte/transition';

    let isLoginForm = false;
    let accountRegistered = false;
    let registerError = false;
    let registerErrorMessage: string;
    let loginError = false;
    let loginErrorMessage: string;
    let username = '';
    let email = '';
    let usernameEmail: string;
    let password = '';
    let confirmPassword = '';
    let birthDate = '';
    let pronouns = '';

    let usernameMessage = ' ';
    let emailMessage = ' ';
    let passwordMessage = ' ';
    let confirmPasswordMessage = ' ';

    onMount(async () => {
        if(await isLoggedIn()){
            await goto('/chat/home')
        }
    });
    $: {
        usernameMessage = validateUsername(username);
        emailMessage = validateEmail(email);
        passwordMessage = validatePassword(password);
        confirmPasswordMessage = validateConfirmPassword(password, confirmPassword);
    }

    let registerSubmittable = false;
    $: {
        if (usernameMessage === ' ' && emailMessage === ' ' && passwordMessage === ' ' && confirmPasswordMessage === ' ' && username !== ' ' && email !== ' ' && password !== '' && confirmPassword !== '') {
            registerSubmittable = false;
        } else {
            registerSubmittable = true;
        }
    }
    let loginSubmittable = false;
    $: {
        if (usernameEmail !== '' && password !== '') {
            loginSubmittable = false;
        } else {
            loginSubmittable = true;
        }
    }
    async function userRegister(event: Event) {
        event.preventDefault();
        const formData = { 
            username, 
            email, 
            password, 
            confirmPassword, 
            birthDate: new Date(birthDate), 
            pronouns
        };
        const response = await handleAccountRegister(formData);
        if(response.userExists) {
            registerError = true;
            registerErrorMessage = 'Потребителското име вече е заето.';
        }
        else if(response.emailExists) {
            registerError = true;
            registerErrorMessage = 'Имейлът вече е зает.';
        }
        else if (response.accountRegistered ) {
            accountRegistered = true;
        }
        
    }
    async function userLogin(event: Event) {
        event.preventDefault();
        const formData = { 
            usernameEmail, 
            password
        };
        const response = await handleAccountLogin(formData);
     
        if (response.invalidCredentials){
            loginError = true;
            loginErrorMessage = 'Невалидни данни.';
        }
        else if (response.token) {
           window.location.href = '/chat/home';
        }

        updateUserInfo();
        
    }
</script>

<div class="wrap">
    <div class="body">        
        <div class="site-info">
            <h1>Boomerang</h1>
            <p>Където думите летят и се връщат!
                Регистрирайте се или си влезте в акаунта ако вече имате такъв
            </p>
        </div>
        <div class="account-form">
            <div class="card shadow-2-strong card-registration" style="border-radius: 15px;">
                <div class="d-flex flex-row justify-content-evenly w-100">
                    <h3 class="form-type-register w-50 h-50 text-center mb-4  pb-md-0 mb-md-5" class:active={!isLoginForm} on:click={() => isLoginForm = false}>Регистрация</h3>
                    <h3 class="form-type-login w-50 h-50 text-center pb-md-0 mb-md-5" class:active={isLoginForm} on:click={() => isLoginForm = true}>Вписване</h3>
                </div>
                <div class="card-body p-4 p-md-5">
                    
                    {#if !isLoginForm}
                    <form in:fade={{ duration: 500 }} on:submit={userRegister}>        
                    <div class="row">
                        <div class="col-md-6 mb-4">
        
                            <div class="form-outline input-wrapper">
                                <input type="text" class:red-outline={usernameMessage != " "} id="username" bind:value={username} class="form-control form-control-lg" />
                                <label class="form-label" for="username">Потребителско име</label>
                                <p class="text-danger error-message"><i>{usernameMessage}</i></p>
                            </div>
                        
                            </div>
                            <div class="col-md-6 mb-4">
                        
                            <div class="form-outline input-wrapper">
                                <input type="text" class:red-outline={emailMessage != " "} id="email" bind:value={email} class="form-control form-control-lg" />
                                <label class="form-label" for="email">Имейл</label>
                                <p class="text-danger error-message"><i>{emailMessage}</i></p>
                            </div>
        
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6 mb-4 d-flex align-items-center">
                        <div class="form-outline datepicker w-100">
                            <input type="date" class="form-control form-control-lg" bind:value={birthDate} id="birthdayDate" />
                            <label for="birthdayDate" class="form-label">Дата на раждане</label>
                        </div>
                        </div>
                        <div class="col-md-6 mb-4">
                        <h6 class="mb-2 pb-1">Пол: </h6>
                        <div class="form-check form-check-inline">
                            <input class="form-check-input" type="radio" bind:group={pronouns} name="inlineRadioOptions" id="femaleGender"
                            value="female" checked />
                            <label class="form-check-label" for="femaleGender">Жена</label>
                        </div>
        
                        <div class="form-check form-check-inline">
                            <input class="form-check-input" type="radio" bind:group={pronouns} name="inlineRadioOptions" id="maleGender"
                            value="male" />
                            <label class="form-check-label" for="maleGender">Мъж</label>
                        </div>
        
                        <div class="form-check form-check-inline">
                            <input class="form-check-input" type="radio" bind:group={pronouns} name="inlineRadioOptions" id="otherGender"
                            value="other" />
                            <label class="form-check-label" for="otherGender">Друг</label>
                        </div>
        
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6 mb-4 pb-2">
                        <div class="form-outline input-wrapper">
                            <input type="password" class:red-outline={passwordMessage != " "} id="password" bind:value={password} class="form-control form-control-lg" />
                            <label class="form-label" for="password">Парола</label>
                            <p class="text-danger error-message" ><i>{passwordMessage}</i></p>
                        </div>
                        </div>
                        <div class="col-md-6 mb-4 pb-2">
                        <div class="form-outline input-wrapper">
                            <input type="password" class:red-outline={confirmPasswordMessage != " "} id="confirmPassword" bind:value={confirmPassword} class="form-control form-control-lg" />
                            <label class="form-label" for="confirmPassword">Потвърждаване на паролата</label>
                            <p class="text-danger error-message" ><i>{confirmPasswordMessage}</i></p>
                        </div>
                        </div>
                    </div>    
                        {#if (accountRegistered)}
                        <div class="alert alert-success" role="alert">
                            Регистрацията е успешна. Моля, влезте в акаунта си.
                        </div>
                        
                        {:else if (registerError)}
                        <div class="alert alert-danger" role="alert">
                            {registerErrorMessage}
                        </div>
                        {/if}    
                        <div class="mt-2 pt-2">
                            <input class="btn btn-primary btn-lg" type="submit" value="Регистриране" disabled={registerSubmittable}/>
                        </div> 
                    </form>
                    {:else}
                    <form on:submit={userLogin}>        
                        <div>
                            <div class="mb-4">
            
                            <div class="form-outline">
                                <input type="text" id="firstName" bind:value={usernameEmail} class="form-control form-control-lg" />
                                <label class="form-label" for="firstName">Потребителско име или имейл</label>
                            </div>
                            <div class="form-outline">
                                <input type="password" id="emailAddress" bind:value={password} class="form-control form-control-lg" />
                                <label class="form-label" for="emailAddress">Парола</label>
                            </div>
                        </div>
                        {#if (loginError)}
                        <div class="alert alert-danger" role="alert">
                            {loginErrorMessage}
                        </div>
                        {/if}
                        <div class="mt-2 pt-2">
                            <input class="btn btn-primary btn-lg" type="submit" value="Вписване" disabled={loginSubmittable}/>
                        </div>
                    </form>
                    {/if}
                </div>
            </div>        
        </div>          
    </div>
</div>

<style>
    .input-wrapper {
    position: relative;
    }

    .mb-4{
        margin-bottom: 1.8rem !important;
    }
    .error-message {
    position: absolute;
    top:70px; /* Adjust as needed */
    left: 0;
    color: red;
    font-size: .8rem;
    }
    .red-outline{
        border-color: red !important;
        box-shadow: 0 0 0 0.25rem rgba(255,0,0,.25) !important;
        transition: border-color .15s ease-in-out,box-shadow .15s ease-in-out;
    }
    .wrap{
        width: 100%;
        height: 100%;
        display:flex;
        flex-direction: row;
        justify-content: center;
        background: linear-gradient(to top, var(--prim), var(--bg));
        overflow: auto;
    }
    .body {
            display: flex;
            flex-direction: row;
            margin:auto;
        }
   
    
        .card-body{
            padding-top:0.2rem !important;
            overflow: auto;
        }
        .site-info{
            word-wrap: break-word;
            width: 100%;
            line-height: 1.2;
            justify-content: center;
            align-items: center;
            
        }
        .account-form{
            width: 100%;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
        }
            .site-info h1{
                font-size: 6rem;
                color: var(--prim-tp);
            }
            .site-info p{
                font-family: "Open Sans",sans-serif;
                font-style: normal;
                font-weight: 400;
                font-size: 1.2rem;
                line-height: 28px;
                color: var(--portal-denim-72);
                margin-bottom: 2rem;
                text-align: unset;
            }
            .form-type-register{
                border-top-left-radius: 15px;
                cursor: pointer;
            }
            .form-type-login{
                border-top-right-radius: 15px;
                cursor: pointer;
            }
            .account-form .active{
                background-color: var(--sec-tp);
                transition: background-color 0.5s;
            }
                    
                .account-form{
                    size-adjust: auto;
                    background-color: none;
                } 


                .card{
                    height:36rem;
                    width: 40rem;   
                }
                @media only screen and (max-width: 600px) {
                   
                    .wrap{
                        padding: 1rem;
                    }
                    .body {
                        display: flex;
                        flex-direction: column;
                        margin:1rem 0 1rem 0;
                    }

                    .site-info h1{
                        font-size: 3rem;
                        color: var(--prim-tp);
                    }
                    .site-info p{
                        font-family: "Open Sans",sans-serif;
                        font-style: normal;
                        font-weight: 400;
                        font-size: 0.8rem;
                        line-height: 28px;
                        color: var(--portal-denim-72);
                        margin-bottom: 2rem;
                        text-align: unset;
                    }
                    .card{
                        height:30rem;
                        width: 100%;   
                    }
                }
</style>