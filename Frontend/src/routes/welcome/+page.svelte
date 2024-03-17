<script lang="ts">
    import { goto } from '$app/navigation';
    import { handleAccountRegister, handleAccountLogin } from '$lib/Handlers/accountHandler';

    let isLoginForm = false;
    let username = '';
    let email = '';
    let password = '';
    let confirmPassword = '';
    let birthDate = '';
    let pronouns = '';
    console.log(import.meta.env.VITE_BACKEND_URL);

    async function handleSubmit(event: Event) {
        event.preventDefault();
        const formData = { 
            username, 
            email, 
            password, 
            confirmPassword, 
            birthDate: new Date(birthDate), 
            pronouns

        };

        const response = isLoginForm ? await handleAccountLogin(formData) : await handleAccountRegister(formData);

        // If the operation was successful, redirect the user to another page
        if (response.ok) {
            console.log("button pressed");
        }
    }

    function switchForm() {
        isLoginForm = !isLoginForm;
    }
</script>

<div class="wrap">
    <div class="center">
        <h1>Добре дошли в Бумеранг</h1>
        <p>където думите пътуват и се връщат!</p>
        <button on:click={switchForm}>
             {isLoginForm ? 'Нуждаете се от регистрация?' : 'Имате регистрация?'}
        </button>
        <form class="account-form" on:submit={handleSubmit} method="POST">
            {#if !isLoginForm}
                <div class="form-group">
                    <label for="username">Потребителско име</label>
                    <input name="username" bind:value={username}>
                </div>
            {/if}
            <div class="form-group">
                <label for="email">Имейл</label>
                <input type="email" name="email" bind:value={email}>
            </div>
            <div class="form-group">
                <label for="password">Парола</label>
                <input type="password" name="password" bind:value={password}>
            </div>
            {#if !isLoginForm}
                <div class="form-group">
                    <label for="confirmPassword">Потвърди парола</label>
                    <input name="confirmPassword" bind:value={confirmPassword}>
                </div>
                <div class="form-group">
                    <label for="birthDate">Дата на раждане</label>
                    <input type="date" name="birthDate" bind:value={birthDate}>
                </div>
                <div class="form-group" style="height:fit-content; padding:0.3rem 0.75rem;">
                    <span>Как да се обръщаме към вас?</span>
                    <div>
                        <input type="radio" name="gender" value="male" bind:group={pronouns}>
                        <label for="male">Той / него</label>
                    </div>
                    <div>
                        <input type="radio" name="gender" value="female" bind:group={pronouns}>
                        <label for="female">Тя / нея</label>
                    </div>
                    <div>
                        <input type="radio" name="gender" value="other" bind:group={pronouns}>
                        <label for="other">Предпочитам да не посочвам</label>
                    </div>
                </div>
            {/if}
            <button type="submit">{isLoginForm ? 'Влез' : 'Създай профил'}</button>
        </form>
    </div>
</div>

<style>
    .wrap{
        width: 100%;
        height: 100%;
        display:flex;
        flex-direction: row;
        justify-content: center;
        background: linear-gradient(to top, var(--prim), var(--bg));
    }
        .center{
            display: flex;
            flex-direction: column;
            align-items: center;
        }
            .account-form{
                width: 45rem;
                background: linear-gradient(to top, var(--prim-tp), var(--sec-tp));
                backdrop-filter: blur(10px);
                border-radius: 8px 8px 0 8px;
                padding:1rem;
                position: relative;
            }
                .form-group{
                    position:relative;
                    height:3.5rem;
                    border:solid var(--sec);
                    border-radius: 5px;
                    margin:1rem;
                }
                    .form-group > label {
                        position: absolute;
                        top:5%;
                        left:2%;
                    }
                    .form-group > input {
                        width:100%;
                        height: 100%;
                        font-size: 1.2rem;
                        padding: 0.5rem;
                        padding-top: 1rem;
                        border:none;
                        border-radius: 6px;
                    }
                .account-form > button{
                    position:absolute;
                    width:8rem;
                    height:4rem;
                    bottom:-4rem;
                    right: 0;

                }

</style>